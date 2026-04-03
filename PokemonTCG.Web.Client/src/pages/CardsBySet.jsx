import React, { useEffect, useState } from "react";
import { useParams, Link } from "react-router-dom";

var ICO_WARN = "\u26A0\uFE0F";

export default function CardsBySet() {
    const { setId } = useParams();
    const [cards, setCards] = useState([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState("");

    const API_URL = import.meta.env.VITE_API_URL;

    useEffect(() => {
        const fetchCards = async () => {
            setLoading(true);
            try {
                const res = await fetch(API_URL + "/api/card/setid/" + setId);
                if (!res.ok) throw new Error("HTTP " + res.status);
                const data = await res.json();
                setCards(data);
            } catch (err) {
                console.error(err);
                setError(ICO_WARN + " The cards could not be loaded.");
            } finally {
                setLoading(false);
            }
        };
        fetchCards();
    }, [setId, API_URL]);

    if (loading) return (
        <div className="loading-screen">
            <div className="pokeball-spinner" />
            <span className="loading-text">Loading Cards\u2026</span>
        </div>
    );

    if (error) return (
        <div className="page-content-v2">
            <div className="msg-error">{error}</div>
        </div>
    );

    const first = cards[0];

    return (
        <div className="page-content-v2">
            {first && (
                <div className="set-header">
                    {first.setImage && (
                        <img src={first.setImage} alt={first.setName} className="set-header-logo" />
                    )}
                    <div className="set-header-info">
                        <div className="set-header-name">{first.setName}</div>
                        <div className="set-header-detail">{first.setSerie} Series</div>
                        <div className="set-header-detail">
                            {"Released "}
                            {new Date(first.releaseDate).toLocaleDateString("en-US", {
                                month: "short", day: "numeric", year: "numeric",
                            })}
                        </div>
                        <div className="set-header-detail">
                            {first.setPrintedTotal + " printed \u00B7 " + first.setTotal + " total"}
                        </div>
                    </div>
                </div>
            )}

            <div className="card-grid-v2">
                {cards.map((card) => (
                    <Link key={card.cardId} to={"/card/" + setId + "/" + card.cardId}>
                        <div className="card-thumb">
                            <img src={card.imageLarge} alt={card.name} />
                        </div>
                    </Link>
                ))}
            </div>
        </div>
    );
}
