import { environment } from '../../environments/environment';

export class utilities {
  public static serverUrl = environment.serverUrl;
  public static getPath(isMultiplayer: boolean): string {
    return isMultiplayer ? 'multiplayer' : 'singleplayer';
  }
}