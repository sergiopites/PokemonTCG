// src/pages/Home.jsx
import { Link } from "react-router-dom";

// src/pages/Home.jsx
// src/pages/Home.jsx
export default function Home() {
    return (
        //<div className="page-content">
            <div
                style={{
                    height: "100vh",
                    margin: 0,
                    padding: 0,
                    overflow: "hidden", // evita cualquier scroll
                }}
            >
                <div
                    style={{
                        //backgroundImage: 'url("/src/images/pokemontcgHome2.png")',
                        backgroundSize: "cover",
                        backgroundPosition: "top center", // 👈 imagen anclada arriba
                        backgroundRepeat: "repeat",
                        backgroundAttachment: "fixed",
                        height: "100%",
                        maxWidth: "1000px",
                        margin: "0 auto",
                        borderRadius: "12px",
                        boxShadow: "0 4px 20px rgba(0,0,0,0.15)",
                        display: "flex",
                        flexDirection: "column",
                        justifyContent: "flex-start", // 👈 contenido arriba
                        alignItems: "center",
                        paddingTop: "40px", // 👈 separa un poco desde arriba
                        boxSizing: "border-box",
                    }}
                >
                    <img
                        src="/src/images/Pokémon_Trading_Card_Game_logo.png"
                        alt="Pokémon TCG Logo"
                        style={{
                            width: "400px",
                            height: "auto",
                            display: "block",
                            marginBottom: "20px",
                        }}
                    />
                    <strong
                        style={{
                            fontSize: "18px",
                            color: "black",
                        }}
                    >
                        Explore cards, build decks, and collect accessories.
                    </strong>
                </div>
            {/*</div>*/}
        //</div>
    );
}


