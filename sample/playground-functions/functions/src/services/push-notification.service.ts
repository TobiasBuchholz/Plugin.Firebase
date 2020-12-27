import * as admin from "firebase-admin";
import { PushNotification } from "../models/push-notifications/push-notification";
import { FirestorePaths } from "../helpers/firestore-paths.helper";

export class PushNotificationService {
  
  public static sendPushNotification(notification: PushNotification): Promise<any> {
    return admin
      .firestore()
      .collection(FirestorePaths.pushNotifications())
      .add(notification.toPlain());
  }
}