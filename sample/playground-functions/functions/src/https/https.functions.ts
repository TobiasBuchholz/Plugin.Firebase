import * as functions from "firebase-functions";

export const echo = functions.https.onRequest((request, response) => {
  functions.logger.log("[+] echo");
  response.send(request.body);
});

export const convertToLeet = functions.https.onCall(async (data, context) =>  {
  functions.logger.log('[+] convertToLeet:', data);
  return `{ "input_value": ${data?.input_value}, "output_value": 1337 }`;
});
