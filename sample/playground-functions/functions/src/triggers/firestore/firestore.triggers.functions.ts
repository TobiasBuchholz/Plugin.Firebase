import * as functions from 'firebase-functions';
import { FirestorePaths } from "../../helpers/firestore-paths.helper";
import { OnPushNotificationCreateFunction } from "./on-push-notification-create.function";

export const onFirestorePushNotificationCreate = functions.firestore.document(`${FirestorePaths.pushNotifications()}/{notificationId}`).onCreate((snap, context) =>
  new OnPushNotificationCreateFunction().execute(snap));

