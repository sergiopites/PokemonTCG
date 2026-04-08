import { BrowserRouter, Routes, Route } from "react-router-dom";
import Layout from "./layouts/Layout";

import Home from "./pages/Home";
import SaveCards from "./pages/SaveCards";
import SaveSets from "./pages/SaveSets";
import AllSets from "./pages/allsets";
import CardDetail from "./pages/carddetail";
import CardsBySet from "./pages/CardsBySet";
import CreateDecks from "./pages/CreateDeck";
import CreateCollection from "./pages/CreateCollection";
import UpdateCollection from "./pages/UpdateCollection";
import UpdateDeck from "./pages/UpdateDeck";
import SearchCard from "./pages/SearchCard";
import ScanCard from "./pages/ScanCard";
import About from "./pages/About";

function PlaceholderPage({ title }) {
    return (
        <div className="page-content-v2">
            <div className="admin-panel">
                <h2>{title}</h2>
                <p>This page is coming soon.</p>
            </div>
        </div>
    );
}

export default function App() {
    return (
        <BrowserRouter>
            <Routes>
                <Route path="/" element={<Layout />}>
                    <Route index element={<Home />} />
                    <Route path="home" element={<Home />} />

                    <Route path="card" element={<PlaceholderPage title="Cards" />} />
                    <Route path="card/:setId" element={<CardsBySet />} />
                    <Route path="card/:setId/:cardId" element={<CardDetail />} />
                    <Route path="card/cardid/:cardId" element={<CardDetail />} />
                    <Route path="card/searchcard" element={<SearchCard />} />
                    <Route path="card/scancard" element={<ScanCard />} />

                    <Route path="createdeck" element={<CreateDecks />} />
                    <Route path="createcollection" element={<CreateCollection />} />
                    <Route path="updatecollection" element={<UpdateCollection />} />
                    <Route path="updatedeck" element={<UpdateDeck />} />
                    <Route path="accessories" element={<PlaceholderPage title="Accessories" />} />
                    <Route path="about" element={<About />} />

                    <Route path="savecards" element={<SaveCards />} />
                    <Route path="savesets" element={<SaveSets />} />
                    <Route path="allsets" element={<AllSets />} />

                    <Route path="*" element={<PlaceholderPage title="404 — Page Not Found" />} />
                </Route>
            </Routes>
        </BrowserRouter>
    );
}




