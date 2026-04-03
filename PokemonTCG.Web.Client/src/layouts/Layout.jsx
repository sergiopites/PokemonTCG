import React, { useState, useEffect, useRef } from "react";
import { Link, Outlet } from "react-router-dom";

const ICON_CARDS = "\uD83C\uDCCF";
const ICON_SETS = "\uD83D\uDCE6";
const ICON_BUILD = "\uD83E\uDDE9";

export default function Layout() {
    const [openMenu, setOpenMenu] = useState(null);
    const navRef = useRef(null);

    const toggle = (menu) => setOpenMenu(openMenu === menu ? null : menu);

    useEffect(() => {
        const close = (e) => {
            if (navRef.current && !navRef.current.contains(e.target)) setOpenMenu(null);
        };
        document.addEventListener("mousedown", close);
        return () => document.removeEventListener("mousedown", close);
    }, []);

    return (
        <>
            <nav className="nav-bar" ref={navRef}>
                <Link to="/" className="nav-logo">
                    <img src="/src/images/pokemon-tcg-logo.png" alt="Pok\u00e9mon TCG" />
                </Link>

                <div className="nav-links">
                    <div className={`nav-item ${openMenu === "cards" ? "open" : ""}`} onClick={() => toggle("cards")}>
                        {ICON_CARDS} Cards {"\u25BE"}
                        <ul className={`nav-dropdown ${openMenu === "cards" ? "show" : ""}`}>
                            <li><Link to="/card/SearchCard" onClick={() => setOpenMenu(null)}>Search Cards</Link></li>
                            <li><Link to="/card/ScanCard" onClick={() => setOpenMenu(null)}>Scan Card</Link></li>
                            <li><Link to="/SaveCards" onClick={() => setOpenMenu(null)}>Sync Cards</Link></li>
                        </ul>
                    </div>

                    <div className={`nav-item ${openMenu === "sets" ? "open" : ""}`} onClick={() => toggle("sets")}>
                        {ICON_SETS} Sets {"\u25BE"}
                        <ul className={`nav-dropdown ${openMenu === "sets" ? "show" : ""}`}>
                            <li><Link to="/allsets" onClick={() => setOpenMenu(null)}>All Sets</Link></li>
                            <li><Link to="/SaveSets" onClick={() => setOpenMenu(null)}>Sync Sets</Link></li>
                        </ul>
                    </div>

                    <div className={`nav-item ${openMenu === "build" ? "open" : ""}`} onClick={() => toggle("build")}>
                        {ICON_BUILD} Build {"\u25BE"}
                        <ul className={`nav-dropdown ${openMenu === "build" ? "show" : ""}`}>
                            <li><Link to="/CreateDeck" onClick={() => setOpenMenu(null)}>Build Deck</Link></li>
                            <li><Link to="/UpdateDeck" onClick={() => setOpenMenu(null)}>Update Deck</Link></li>
                            <li><Link to="/CreateCollection" onClick={() => setOpenMenu(null)}>New Collection</Link></li>
                            <li><Link to="/UpdateCollection" onClick={() => setOpenMenu(null)}>Update Collection</Link></li>
                        </ul>
                    </div>

                    <div className="nav-item">
                        <Link to="/about">About</Link>
                    </div>
                </div>
            </nav>

            <div className="page-shell">
                <Outlet />
            </div>
        </>
    );
}
