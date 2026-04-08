import tcgLogo from "../images/Pokemon_Trading_Card_Game_logo.png";

export default function About() {
    return (
        <div className="page-content-v2">
            <h1 className="section-title">About This Project</h1>

            <div className="about-layout">
                <div className="about-image-wrap">
                    <img
                        src={tcgLogo}
                        alt="Pok&eacute;mon TCG Logo"
                        className="about-logo"
                    />
                </div>

                <div className="glass-panel about-text">
                    <h2 className="about-heading">
                        {"\u2728"} Open Source &middot; By Fans &middot; For Fans
                    </h2>

                    <p>
                        This is a <strong>free and open-source</strong> project built entirely by fans of the
                        Pok&eacute;mon Trading Card Game. It was created to help players explore cards, build
                        decks, manage collections, and enjoy the TCG hobby in a modern, accessible way.
                    </p>

                    <p>
                        All card data is sourced from public community APIs. No proprietary content is hosted
                        or distributed by this application.
                    </p>

                    <div className="about-disclaimer">
                        <h3>{"\u26A0\uFE0F"} Disclaimer</h3>
                        <p>
                            This project is <strong>not affiliated with, endorsed by, or connected to</strong> The
                            Pok&eacute;mon Company, Nintendo, Game Freak, Creatures Inc., or any of their
                            subsidiaries or affiliates.
                        </p>
                        <p>
                            Pok&eacute;mon and all related names, logos, and imagery are trademarks and
                            &copy; of their respective owners. This fan project is provided &ldquo;as
                            is&rdquo; for personal, non-commercial use only.
                        </p>
                    </div>

                    <div className="about-links">
                        <a
                            href="https://github.com/sergiopites/PokemonTCG"
                            target="_blank"
                            rel="noopener noreferrer"
                            className="btn btn-outline btn-sm"
                        >
                            {"\uD83D\uDCBB"} View on GitHub
                        </a>
                    </div>
                </div>
            </div>
        </div>
    );
}
