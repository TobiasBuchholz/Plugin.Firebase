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

exports.echo = functions.https.onRequest(async (request, response) => {
    functions.logger.log(`[+] echo: headers = ${JSON.stringify(request.headers)}`);
    response.send(request.body);
});
