import * as admin from "firebase-admin";
import * as functions from "firebase-functions";

export const echo = functions.https.onRequest(async (request, response) => {
  functions.logger.log("[+] echo");
  response.send(request.body);
});

export const addCustomClaimToUser = functions.https.onRequest(async (request, response) => {
    functions.logger.log(`[+] addCustomClaimToUser: uid = ${request.query.uid}`);
    if(request.query.uid) {
        await admin
            .auth()
            .setCustomUserClaims(request.query.uid.toString(), { is_awesome: true });
    }
    response.send();
});