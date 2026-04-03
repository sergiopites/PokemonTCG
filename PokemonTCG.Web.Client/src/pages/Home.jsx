import { Link } from "react-router-dom";
import tcgLogo from "../images/Pokemon_Trading_Card_Game_logo.png";

export default function Home() {
    return (
        <div className="home-hero">
            <img
                src={tcgLogo}
                alt="Pok\u00e9mon TCG Logo"
                className="home-logo-img"
            />
            <p className="home-tagline">
                Explore cards &middot; Build decks &middot; Collect them all
            </p>
            <div className="home-actions">
                <Link to="/card/SearchCard" className="btn btn-gold">
                    {"\uD83D\uDD0D"} Search Cards
                </Link>
                <Link to="/allsets" className="btn btn-outline">
                    {"\uD83D\uDCE6"} Browse Sets
                </Link>
                <Link to="/CreateDeck" className="btn btn-blue">
                    {"\uD83E\uDDE9"} Build Deck
                </Link>
                <Link to="/CreateCollection" className="btn btn-outline">
                    {"\u2B50"} New Collection
                </Link>
            </div>
        </div>
    );
}
