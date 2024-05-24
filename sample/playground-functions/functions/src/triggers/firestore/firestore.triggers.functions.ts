import * as functions from 'firebase-functions/v1';
import { FirestorePaths } from "../../helpers/firestore-paths.helper";
import { OnPushNotificationCreateFunction } from "./on-push-notification-create.function";

export const onFirestorePushNotificationCreate = functions.firestore.document(`${FirestorePaths.pushNotifications()}/{notificationId}`).onCreate((snap, context) =>
  new OnPushNotificationCreateFunction().execute(snap));

