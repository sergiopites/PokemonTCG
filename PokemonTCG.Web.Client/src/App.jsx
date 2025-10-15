/* src/App.jsx */
//import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
//import Home from "./pages/Home";
//import Cards from "./pages/SaveCards";
//import AllSets from "./pages/allsets";
//import CardDetail from "./pages/CardDetail";
//import CardsBySet from "./pages/CardsBySet";

//function App() {
//    return (
//<Router>
//    <Routes>
//        <Route path="/" element={<Home />} />
//        <Route path="/savecards" element={<Cards />} />
//        <Route path="/cards/:setId" element={<CardsBySet />} />
//        <Route path="/allsets" element={<AllSets />} />
//        <Route path="/card/:cardId" element={<CardDetail />} />
//        <Route path="/cards/:setId/card/:cardId" element={<CardDetail />} />
//    </Routes>
//</Router>
//    );
//}

//export default App;
// src/App.jsx
import { BrowserRouter, Routes, Route } from "react-router-dom";
import Layout from "./layouts/Layout";
import Home from "./pages/Home";
import Cards from "./pages/SaveCards";
import Sets from "./pages/SaveSets";
import AllSets from "./pages/allsets";
import CardDetail from "./pages/CardDetail";
import CardsBySet from "./pages/CardsBySet";

export default function App() {
    return (
        <BrowserRouter>
            <Routes>
                <Route path="/" element={<Layout />}>
                    <Route index element={<Home />} />
                    <Route path="home" element={<Home />} />
                    <Route path="cards" element={<div>Cards Page</div>} />                    
                    <Route path="cards/:setId" element={<CardsBySet />} />
                    <Route path="cards/:setId/card/:cardId" element={<CardDetail />} />
                    <Route path="cards/:cardId" element={<CardDetail />} />
                    <Route path="decks" element={<div>Decks Page</div>} />
                    <Route path="accessories" element={<div>Accessories Page</div>} />
                    <Route path="about" element={<div>About Page</div>} />
                    <Route path="savecards" element={<Cards />} />                    
                    <Route path="savesets" element={<Sets />} />  
                    <Route path="allsets" element={<AllSets />} />
                </Route>
            </Routes>
        </BrowserRouter>
    );
}




