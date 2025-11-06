import React, { useEffect, useState } from "react";
import { useParams, Link } from "react-router-dom";
import "../global.css";

export default function CardsBySet() {
    const { setId } = useParams();   // 👈 obtiene el setId de la URL
    const [cards, setCards] = useState([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState("");

    const API_URL = import.meta.env.VITE_API_URL;

    useEffect(() => {
        const fetchCards = async () => {
            setLoading(true);
            try {
                console.log(`URL Completa:` + `${API_URL}/api/card/setid/${setId}`);
                const res = await fetch(`${API_URL}/api/card/setid/${setId}`);
                if (!res.ok) throw new Error(`HTTP ${res.status}`);
                const data = await res.json();
                setCards(data);
            } catch (err) {
                console.error(err);
                setError("⚠️ The cards could not be loaded.");
            } finally {
                setLoading(false);
            }
        };
        fetchCards();
    }, [setId]);

    useEffect(() => {
        if (cards.length > 0) {
            console.log(cards[0]);
        }
    }, [cards]);

    /* ------------------- Render ------------------- */
    if (loading) return <div className="loading">⏳ Loading...</div>;
    if (error) return <div className="error">{error}</div>;

    return (
        <div className="page-content">
            <div className="all-cards">

                {/* 👇 Logo del set arriba */}
                <table width="100%">
                    <tbody>
                        <tr>
                            <td width="50%">
                                {cards.length > 0 && (
                                    <div className="set-logo pokemon-tcg-text">
                                        <img
                                            src={cards[0].setImage}
                                            alt={cards[0].setName}
                                            className="set-image"
                                        />                                       
                                    </div>
                                )}
                            </td>
                            {/*<td width="50%" className="card-detail-info text-2xl font-bold" style={{ textAlign: "left", verticalAlign: "middle", }}>*/}
                            <td width="50%" className="card-detail-info text-2xl">
                                {cards.length > 0 && (
                                    //<div className="text-2xl font-bold" style={{ color: "black" }}>
                                    <div className="pokemon-tcg-text" >
                                        <table width="100%"><tr><td>
                                            <h2 >
                                                {cards[0].setName}
                                            </h2>
                                            <span>{cards[0].setSerie} Series</span>
                                        </td>
                                        </tr>
                                            <tr>
                                                <td>
                                                    <span>
                                                        Release Date {" "}
                                                        {cards.length > 0 &&
                                                            new Date(cards[0].releaseDate).toLocaleDateString("es-AR", {
                                                                day: "2-digit",
                                                                month: "2-digit",
                                                                year: "numeric",
                                                            })}
                                                    </span>
                                                </td></tr>
                                            <tr><td>
                                                <span>{cards[0].setPrintedTotal} printed cards</span>
                                            </td></tr>
                                            <tr><td>
                                                <span>{cards[0].setTotal} total cards</span>
                                            </td></tr></table>
                                    </div>
                                )}
                            </td>
                        </tr>
                    </tbody>
                </table>
                <div className="card-grid">
                    {cards.map((card) => (
                        <div key={card.cardId} className="card-item">
                            <Link to={`/card/${setId}/${card.cardId}`}>
                                <img
                                    src={card.imageLarge}
                                    alt={card.name}
                                    className="card-image"
                                />
                            </Link>
                        </div>
                    ))}
                </div>
            </div>
        </div>
    );
}