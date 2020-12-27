import { classToPlain, deserialize, Expose } from "class-transformer";
import { PushNotificationType } from "./push-notification-type";

export class PushNotification {
  
  public static FromData(data: any): PushNotification {
    return deserialize(PushNotification, JSON.stringify(data));
  }

  public static FromTopic(topic: string, title: string, body: string): PushNotification {
    return new PushNotification(PushNotificationType.TOPIC, title, body, topic, Array.of());
  }

  public static FromTokens(fcmTokens: string[], title: string, body: string): PushNotification {
    return new PushNotification(PushNotificationType.TOKENS, title, body, '', fcmTokens);
  }
  
  constructor(
    type: PushNotificationType,
    title: string,
    body: string,
    topic: string,
    fcmTokens: string[]) {
    
    this.type = type;
    this.title = title;
    this.body = body;
    this.topic = topic;
    this.fcmTokens = fcmTokens;
  }
  
  public toPlain(): object {
    return classToPlain(this);
  }

  @Expose({ name: "type"})
  type: PushNotificationType;
  
  @Expose({ name: "title"})
  title: string;
  
  @Expose({ name: "body"})
  body: string;

  @Expose({ name: "topic"})
  topic: string;
  
  @Expose({ name: "fcm_tokens"})
  fcmTokens: string[];
}