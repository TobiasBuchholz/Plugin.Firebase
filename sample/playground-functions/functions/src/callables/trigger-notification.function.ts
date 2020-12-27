import { PushNotification } from "../models/push-notifications/push-notification";
import { PushNotificationService } from "../services/push-notification.service";

export class TriggerNotificationFunction {

  public static async execute(requestData: any): Promise<any> {
    const notification = PushNotification.FromData(requestData);
    return PushNotificationService.sendPushNotification(notification);
  }
}
