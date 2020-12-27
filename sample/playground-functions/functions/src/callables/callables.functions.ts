import * as functions from 'firebase-functions';
import { TriggerNotificationFunction } from "./trigger-notification.function";

export const triggerNotification = functions.https.onCall(async (data, context) =>
  TriggerNotificationFunction.execute(data));