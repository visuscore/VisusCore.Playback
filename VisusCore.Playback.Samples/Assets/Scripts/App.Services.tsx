import { Container } from "./lib/inversify-fix";
import PlaybackImageService from "./services/PlaybackImage.Service";
import PlaybackStreamInfoService from "./services/PlaybackStreamInfo.Service";
import StorageStreamInfoService from "./services/StorageStreamInfo.Service";

const container = new Container();

container.bind<PlaybackStreamInfoService>(PlaybackStreamInfoService).toSelf().inSingletonScope();
container.bind<PlaybackImageService>(PlaybackImageService).toSelf().inSingletonScope();
container.bind<StorageStreamInfoService>(StorageStreamInfoService).toSelf().inSingletonScope();

export default container;
