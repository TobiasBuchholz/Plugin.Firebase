import * as admin from "firebase-admin";
import { classToPlain } from "class-transformer";
import { QueryDocumentSnapshot } from "firebase-functions/lib/providers/firestore";
import { PushNotification } from "../../models/push-notifications/push-notification";
import { PushNotificationType } from "../../models/push-notifications/push-notification-type";
import { HttpsError } from "firebase-functions/lib/providers/https";

export class OnPushNotificationCreateFunction {

  public async execute(snapshot: QueryDocumentSnapshot): Promise<any> {
    const notification = PushNotification.FromData(snapshot.data());
    const sendDate = new Date(Date.now());
    const result = await this.sendNotification(notification);
    await snapshot.ref.update({ 
      send_result: classToPlain(result),
      send_date: sendDate
    });
  }
  
  private sendNotification(notification: PushNotification): Promise<any> {
    switch(notification.type) {
      case PushNotificationType.TOKENS:
        return this.sendNotificationViaTokens(notification);
      case PushNotificationType.TOPIC:
        return this.sendNotificationViaTopic(notification);
      default:
        throw new HttpsError('invalid-argument', `notification type is not supported: ${notification.type}`);
    }
  }
  
  private sendNotificationViaTokens(notification: PushNotification): Promise<any> {
    const message = this.createMessage(notification);
    message.tokens = notification.fcmTokens;
    return admin.messaging().sendMulticast(message);
  }
  
  private createMessage(notification: PushNotification): any {
    return {
      notification: {
        title: notification.title,
        body: notification.body,
        image: 'https://picsum.photos/200'
      },
      data: {
        title: notification.title,
        body: notification.body
      }
    };
  }
  
  private sendNotificationViaTopic(notification: PushNotification): Promise<any> {
    const message = this.createMessage(notification);
    message.topic = notification.topic;
    return admin.messaging().send(message);
  }
}