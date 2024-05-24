import * as functions from 'firebase-functions/v1';
import { TriggerNotificationFunction } from "./trigger-notification.function";

export const convertToLeet = functions.https.onCall(async (data, context) =>  {
    functions.logger.log('[+] convertToLeet:', data);
    return `{ "input_value": ${data?.input_value}, "output_value": 1337 }`;
});

export const triggerNotification = functions.https.onCall(async (data, context) => {
    functions.logger.log('[+] triggerNotification:', data);
    return TriggerNotificationFunction.execute(data);
});