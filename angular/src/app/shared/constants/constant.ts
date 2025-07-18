import { environment } from "src/environments/environment";

export class Constants {
    static EnvironmentUrl = environment.apis.default.url;
    static PageSizeOption = [10, 25, 50, 100];
    static NotificationHubUrl = '/signalr-hubs/notification'
    static EntityHubUrl = '/signalr-hubs/entity';
}