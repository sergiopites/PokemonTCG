import { BrowserRouter, Routes, Route } from "react-router-dom";
import Layout from "./layouts/Layout";

// 🧩 Pages
import Home from "./pages/Home";
import SaveCards from "./pages/SaveCards";
import SaveSets from "./pages/SaveSets";
import AllSets from "./pages/allsets";
import CardDetail from "./pages/carddetail";
import CardsBySet from "./pages/CardsBySet";
import CreateDecks from "./pages/CreateDeck";
import SearchCard from "./pages/SearchCard";
export default function App() {
    return (
        <BrowserRouter>
            <Routes>
                <Route path="/" element={<Layout />}>
                    {/* Página principal */}
                    <Route index element={<Home />} />
                    <Route path="home" element={<Home />} />

                    {/* Páginas de cartas */}
                    <Route path="card" element={<div>Cards Page</div>} />
                    <Route path="card/:setId" element={<CardsBySet />} />
                    <Route path="card/:setId/:cardId" element={<CardDetail />} />
                    <Route path="card/:cardId" element={<CardDetail />} />
                    <Route path="card/searchcard" element={<SearchCard />} />

                    {/* Páginas secundarias */}
                    <Route path="createdeck" element={<CreateDecks />} />
                    <Route path="accessories" element={<div>Accessories Page</div>} />
                    <Route path="about" element={<div>About Page</div>} />

                    {/* Administración */}
                    <Route path="savecards" element={<SaveCards />} />
                    <Route path="savesets" element={<SaveSets />} />

                    {/* Listado general de sets */}
                    <Route path="allsets" element={<AllSets />} />

                    {/* Ruta por defecto (404) */}
                    <Route path="*" element={<div>404 - Página no encontrada</div>} />
                </Route>
            </Routes>
        </BrowserRouter>
    );
}




