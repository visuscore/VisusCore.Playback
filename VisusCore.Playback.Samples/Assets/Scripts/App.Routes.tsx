import React from "react";
import { Route, Routes, useParams } from 'react-router-dom';
import { StreamsPage } from "./pages/Streams.Page";
import { StreamLivePage } from "./pages/StreamLive.Page";
import { StreamPlaybackPage } from "./pages/StreamPlayback.Page";
import { StreamImagesPage } from "./pages/StreamImages.Page";

export default (
    <Routes>
        <Route path="/" element={<StreamsPage />} />
        <Route path="/:streamId/live"
                Component={() => {
                    return (<StreamLivePage streamId={useParams().streamId} />)
                }} />
        <Route path="/:streamId/playback"
                Component={() => {
                    return (<StreamPlaybackPage streamId={useParams().streamId} />)
                }} />
        <Route path="/:streamId/images"
                Component={() => {
                    return (<StreamImagesPage streamId={useParams().streamId} />)
                }} />
    </Routes>
);
