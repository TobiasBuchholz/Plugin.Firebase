import * as admin from 'firebase-admin';
import * as functions from 'firebase-functions/v1';

admin.initializeApp();

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

exports.echo = functions.https.onRequest(async (request, response) => {
    functions.logger.log(`[+] echo: headers = ${JSON.stringify(request.headers)}`);
    response.send(request.body);
});
