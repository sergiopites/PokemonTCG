import React, { useState } from "react";
import { Link, Outlet } from "react-router-dom";
import { Layers, Star, Save, Package, Info } from "lucide-react";

export default function TableMenu() {
    const [openMenu, setOpenMenu] = useState(null);

    const toggleMenu = (menu) => {
        setOpenMenu(openMenu === menu ? null : menu);
    };

    return (
        <div
            style={{
                width: "100%",
                backgroundColor: "white",
                boxShadow: "0 2px 4px rgba(0,0,0,0.1)",
                position: "fixed",
                top: 0,
                left: 0,
                zIndex: 1000,
            }}
        >
            <table
                width="100%"
                cellPadding="12"
                style={{
                    tableLayout: "fixed",
                    background: "linear-gradient(90deg, #f3f4f6, #e5e7eb)",
                    color: "#333",
                    boxShadow: "0 2px 6px rgba(0,0,0,0.15)",
                }}
            >
                <tbody>
                    <tr>
                        {/* Logo */}
                        <td style={{ textAlign: "center", verticalAlign: "middle" }}>
                            <Link to="/">
                                <img
                                    src="/src/images/Pokémon_Trading_Card_Game_logo.png"
                                    alt="Pokémon TCG Logo"
                                    style={{ width: "100px", height: "auto" }}
                                />
                            </Link>
                        </td>

                        {/* Cards */}
                        <td
                            style={{
                                textAlign: "center",
                                verticalAlign: "middle",
                                position: "relative",
                                backgroundColor: "#f8fafc",
                                fontWeight: "bold",
                                cursor: "pointer",
                                color: "#2563eb",
                            }}
                            onClick={() => toggleMenu("cards")}
                        >
                            <span>🃏 Cards ▾</span>
                            {openMenu === "cards" && (
                                <ul
                                    style={{
                                        position: "absolute",
                                        top: "100%",
                                        left: "50%",
                                        transform: "translateX(-50%)",
                                        backgroundColor: "#ffffff",
                                        listStyle: "none",
                                        padding: "0",
                                        margin: "6px 0 0",
                                        boxShadow: "0 4px 10px rgba(0,0,0,0.15)",
                                        borderRadius: "10px",
                                        width: "180px",
                                        zIndex: 1000,
                                    }}
                                >
                                    <li className="px-4 py-2 hover:bg-gray-100 text-sm flex items-center gap-2">
                                        <Link to="/SaveCards" style={{ color: "#1d4ed8", fontWeight: "500" }}>
                                            Save/Update cards
                                        </Link>
                                    </li>
                                    <li className="px-4 py-2 hover:bg-gray-100 text-sm flex items-center gap-2">
                                        <Link to="/card/SearchCard" style={{ color: "#ca8a04", fontWeight: "500" }}>
                                            Search
                                        </Link>
                                    </li>
                                    <li className="px-4 py-2 hover:bg-gray-100 text-sm flex items-center gap-2">
                                        <Link to="/card/favorites" style={{ color: "#ca8a04", fontWeight: "500" }}>
                                            Favorites
                                        </Link>
                                    </li>
                                </ul>
                            )}
                        </td>

                        {/* Sets */}
                        <td
                            style={{
                                textAlign: "center",
                                verticalAlign: "middle",
                                position: "relative",
                                backgroundColor: "#f8fafc",
                                fontWeight: "bold",
                                cursor: "pointer",
                                color: "#7c3aed",
                            }}
                            onClick={() => toggleMenu("sets")}
                        >
                            <span>📦 Sets ▾</span>
                            {openMenu === "sets" && (
                                <ul
                                    style={{
                                        position: "absolute",
                                        top: "100%",
                                        left: "50%",
                                        transform: "translateX(-50%)",
                                        backgroundColor: "#ffffff",
                                        listStyle: "none",
                                        padding: "0",
                                        margin: "6px 0 0",
                                        boxShadow: "0 4px 10px rgba(0,0,0,0.15)",
                                        borderRadius: "10px",
                                        width: "180px",
                                        zIndex: 1000,
                                    }}
                                >
                                    <li className="px-4 py-2 hover:bg-gray-100 text-sm flex items-center gap-2">
                                        <Link to="/SaveSets" style={{ color: "#7e22ce", fontWeight: "500" }}>
                                            Save/Update sets
                                        </Link>
                                    </li>
                                    <li className="px-4 py-2 hover:bg-gray-100 text-sm flex items-center gap-2">
                                        <Link to="/allsets" style={{ color: "#6d28d9", fontWeight: "500" }}>
                                            All sets
                                        </Link>
                                    </li>
                                </ul>
                            )}
                        </td>

                        {/* Decks */}
                        <td
                            style={{
                                textAlign: "center",
                                verticalAlign: "middle",
                                position: "relative",
                                backgroundColor: "#f8fafc",
                                fontWeight: "bold",
                                cursor: "pointer",
                                color: "#059669",
                            }}
                            onClick={() => toggleMenu("decks")}
                        >
                        <span>🧩 Decks & Collections ▾</span>
                            {openMenu === "decks" && (
                                <ul
                                    style={{
                                        position: "absolute",
                                        top: "100%",
                                        left: "50%",
                                        transform: "translateX(-50%)",
                                        backgroundColor: "#ffffff",
                                        listStyle: "none",
                                        padding: "0",
                                        margin: "6px 0 0",
                                        boxShadow: "0 4px 10px rgba(0,0,0,0.15)",
                                        borderRadius: "10px",
                                        width: "200px",
                                        zIndex: 1000,
                                    }}
                                >
                                    <li className="px-4 py-2 hover:bg-gray-100 text-sm flex items-center gap-2">
                                        <Link to="/CreateDeck" style={{ color: "#059669", fontWeight: "500" }}>
                                            Build Deck
                                        </Link>
                                    </li>
                                    <li className="px-4 py-2 hover:bg-gray-100 text-sm flex items-center gap-2">
                                        <Link to="/CreateCollection" style={{ color: "#0369a1", fontWeight: "500" }}>
                                            Build Collection
                                        </Link>
                                    </li>
                                </ul>
                            )}
                        </td>

                        {/* Accessories */}
                        <td
                            style={{
                                textAlign: "center",
                                verticalAlign: "middle",
                                backgroundColor: "#f8fafc",
                                fontWeight: "bold",
                            }}
                        >
                            <Link
                                to="/accessories"
                                className="hover:underline"
                                style={{ color: "#dc2626", display: "inline-flex", alignItems: "center", gap: "4px" }}
                            >
                                <Package size={16} color="#ef4444" /> Accessories
                            </Link>
                        </td>

                        {/* About */}
                        <td
                            style={{
                                textAlign: "center",
                                verticalAlign: "middle",
                                backgroundColor: "#f8fafc",
                                fontWeight: "bold",
                            }}
                        >
                            <Link
                                to="/about"
                                className="hover:underline"
                                style={{ color: "#2563eb", display: "inline-flex", alignItems: "center", gap: "4px" }}
                            >
                                <Info size={16} color="#3b82f6" /> About
                            </Link>
                        </td>
                    </tr>
                </tbody>
            </table>

            {/* Contenido principal debajo del menú */}
            <div
                style={{
                    paddingTop: "5px",
                    maxHeight: "100vh",
                    overflowY: "auto",
                }}
            >
                <Outlet />
            </div>
        </div>
    );
}
