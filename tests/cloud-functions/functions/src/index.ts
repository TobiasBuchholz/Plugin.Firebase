import * as admin from 'firebase-admin';
import * as functions from 'firebase-functions/v1';

admin.initializeApp();

function encodeBase64Url(value: object): string {
  return Buffer
    .from(JSON.stringify(value))
    .toString('base64')
    .replace(/\+/g, '-')
    .replace(/\//g, '_')
    .replace(/=+$/g, '');
}

function createUnsignedEmulatorCustomToken(uid: string, claims: object): string {
  const now = Math.floor(Date.now() / 1000);
  const serviceAccount = 'firebase-adminsdk@demo-pluginfirebase-integrationtests.iam.gserviceaccount.com';
  const header = {
    alg: 'none',
    typ: 'JWT',
  };
  const payload = {
    iss: serviceAccount,
    sub: serviceAccount,
    aud: 'https://identitytoolkit.googleapis.com/google.identity.identitytoolkit.v1.IdentityToolkit',
    iat: now,
    exp: now + 3600,
    uid,
    claims,
  };

  return `${encodeBase64Url(header)}.${encodeBase64Url(payload)}.`;
}

exports.addMessage = functions.https.onRequest(async (req, res) => {
  const original = req.query.text;
  const writeResult = await admin.firestore().collection('messages').add( { original: original });
  res.json({ result: `Message with ID: ${ writeResult.id } added.` });
});

exports.makeUppercase = functions.firestore.document('/messages/{documentId}').onCreate((snap, context) => {
  const original = snap.data().original;
  functions.logger.log('[+] makeUppercase:', context.params.documentId, original);

  const uppercase = original.toUpperCase();
  return snap.ref.set({ uppercase }, { merge: true });
});

exports.convertToLeet = functions.https.onCall(async (data, context) =>  {
  functions.logger.log('[+] convertToLeet:', data);
  return `{ "input_value": ${data?.input_value}, "output_value": 1337 }`;
});

exports.regionalPing = functions.region('southamerica-east1').https.onCall(async () => {
  functions.logger.log('[+] regionalPing');
  return '{ "input_value": 0, "output_value": 541 }';
});

exports.returnObjectPayload = functions.https.onCall(async (data, context) =>  {
  functions.logger.log('[+] returnObjectPayload:', data);
  const inputValue = data?.input_value ?? 0;
  return {
    input_value: inputValue,
    output_value: 1337,
    message: 'object response',
    is_valid: true,
    nested: {
      name: 'nested response',
      count: 2,
    },
    items: [
      {
        title: 'first',
        value: 1,
      },
      {
        title: 'second',
        value: 2,
      },
    ],
    tags: ['alpha', 'beta'],
    scores: [3, 5, 8],
  };
});

exports.returnArrayPayload = functions.https.onCall(async (data, context) =>  {
  functions.logger.log('[+] returnArrayPayload:', data);
  return [
    {
      title: 'first',
      value: 1,
    },
    {
      title: 'second',
      value: 2,
    },
  ];
});

exports.returnStringPayload = functions.https.onCall(async (data, context) =>  {
  functions.logger.log('[+] returnStringPayload:', data);
  return 'callable-string';
});

exports.returnEscapedStringPayload = functions.https.onCall(async (data, context) =>  {
  functions.logger.log('[+] returnEscapedStringPayload:', data);
  return 'escaped "quote" and backslash \\\\ path';
});

exports.returnNumberPayload = functions.https.onCall(async (data, context) =>  {
  functions.logger.log('[+] returnNumberPayload:', data);
  return 42;
});

exports.returnBooleanPayload = functions.https.onCall(async (data, context) =>  {
  functions.logger.log('[+] returnBooleanPayload:', data);
  return true;
});

exports.returnNullPayload = functions.https.onCall(async (data, context) =>  {
  functions.logger.log('[+] returnNullPayload:', data);
  return null;
});

exports.createCustomToken = functions.https.onCall(async (data, context) =>  {
  functions.logger.log('[+] createCustomToken:', data);
  if (process.env.FUNCTIONS_EMULATOR !== 'true') {
    throw new functions.https.HttpsError(
      'failed-precondition',
      'createCustomToken is only available when running in the Firebase Functions emulator.'
    );
  }

  const uid = data?.uid ?? `acceptance-${Date.now()}`;
  const claims = data?.claims ?? {};
  let token: string;
  try {
    token = await admin.auth().createCustomToken(uid, claims);
  } catch (error) {
    functions.logger.warn('[!] createCustomToken falling back to unsigned emulator token:', error);
    token = createUnsignedEmulatorCustomToken(uid, claims);
  }
  return {
    uid,
    token,
  };
});

exports.echoAuthContext = functions.https.onCall(async (data, context) =>  {
  functions.logger.log('[+] echoAuthContext:', {
    data,
    uid: context.auth?.uid,
  });
  return {
    has_auth: !!context.auth,
    uid: context.auth?.uid ?? null,
    token_email: context.auth?.token?.email ?? null,
    input_value: data?.input_value ?? null,
  };
});

exports.throwStructuredError = functions.https.onCall(async () =>  {
  functions.logger.log('[+] throwStructuredError');
  throw new functions.https.HttpsError(
    'failed-precondition',
    'Structured acceptance-test failure',
    { reason: 'acceptance-test' }
  );
});

exports.echo = functions.https.onRequest(async (request, response) => {
    functions.logger.log(`[+] echo: headers = ${JSON.stringify(request.headers)}`);
    response.send(request.body);
});
