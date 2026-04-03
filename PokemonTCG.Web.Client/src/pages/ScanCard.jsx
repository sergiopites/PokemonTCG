import React, { useState, useRef, useCallback, useEffect } from "react";
import { Link } from "react-router-dom";

var ICO_CAM    = "\uD83D\uDCF7";
var ICO_SCAN   = "\uD83D\uDD0D";
var ICO_WARN   = "\u26A0\uFE0F";
var ICO_OK     = "\u2705";
var ICO_FAIL   = "\u274C";
var ICO_STOP   = "\u23F9";
var ICO_RETRY  = "\uD83D\uDD04";
var ICO_UPLOAD = "\uD83D\uDCC2";
var ICO_FLIP   = "\uD83D\uDD00";

function DataRow({ label, value }) {
    if (!value || (Array.isArray(value) && value.length === 0)) return null;
    var display = Array.isArray(value) ? value.join(", ") : value;
    return (
        <div style={{ display: "flex", gap: 8, padding: "4px 0", borderBottom: "1px solid rgba(255,255,255,.06)" }}>
            <span style={{ minWidth: 100, color: "var(--gold)", fontFamily: "var(--font-title)", fontSize: ".72rem", textTransform: "uppercase", letterSpacing: ".5px" }}>
                {label}
            </span>
            <span style={{ color: "var(--text)", fontSize: ".85rem" }}>
                {display}
            </span>
        </div>
    );
}

export default function ScanCard() {
    var API_URL = import.meta.env.VITE_API_URL || "";

    var videoRef = useRef(null);
    var canvasRef = useRef(null);
    var streamRef = useRef(null);
    var fileInputRef = useRef(null);

    var [cameraActive, setCameraActive] = useState(false);
    var [scanning, setScanning] = useState(false);
    var [capturedImage, setCapturedImage] = useState(null);
    var [scannedData, setScannedData] = useState(null);
    var [cards, setCards] = useState([]);
    var [message, setMessage] = useState("");
    var [msgType, setMsgType] = useState("");
    var [facingMode, setFacingMode] = useState("environment"); // "environment" = trasera, "user" = frontal

    // Cleanup camera on unmount
    useEffect(function() {
        return function() {
            if (streamRef.current) {
                streamRef.current.getTracks().forEach(function(t) { t.stop(); });
            }
        };
    }, []);

    // ?? Start camera with high-res + autofocus constraints ??
    var startCamera = useCallback(async function(facing) {
        var mode = facing || facingMode;
        try {
            // Stop any existing stream first
            if (streamRef.current) {
                streamRef.current.getTracks().forEach(function(t) { t.stop(); });
            }

            var stream = await navigator.mediaDevices.getUserMedia({
                video: {
                    facingMode: { ideal: mode },
                    width: { ideal: 1920 },
                    height: { ideal: 1440 },
                    focusMode: { ideal: "continuous" },
                    // Prevents torch/flash but helps with auto-exposure
                    exposureMode: { ideal: "continuous" },
                    whiteBalanceMode: { ideal: "continuous" },
                }
            });
            streamRef.current = stream;
            if (videoRef.current) {
                videoRef.current.srcObject = stream;
            }
            setCameraActive(true);
            setCapturedImage(null);
            setCards([]);
            setScannedData(null);
            setMessage("");
            setMsgType("");
        } catch (err) {
            console.error(err);
            setMessage(ICO_WARN + " Could not access camera. Please allow camera permissions.");
            setMsgType("warning");
        }
    }, [facingMode]);

    var stopCamera = useCallback(function() {
        if (streamRef.current) {
            streamRef.current.getTracks().forEach(function(t) { t.stop(); });
            streamRef.current = null;
        }
        if (videoRef.current) {
            videoRef.current.srcObject = null;
        }
        setCameraActive(false);
    }, []);

    // ?? Flip between front and back camera ??
    var flipCamera = useCallback(function() {
        var next = facingMode === "environment" ? "user" : "environment";
        setFacingMode(next);
        if (cameraActive) {
            startCamera(next);
        }
    }, [facingMode, cameraActive, startCamera]);

    // ?? Send image (base64 dataUrl) to backend OCR ??
    var sendToScan = useCallback(async function(dataUrl) {
        setScanning(true);
        setMessage("Reading entire card with OCR\u2026");
        setMsgType("info");

        try {
            var r = await fetch(API_URL + "/api/scanner/scan", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ imageBase64: dataUrl })
            });

            if (!r.ok) {
                var errorText = await r.text();
                throw new Error(errorText || ("HTTP " + r.status));
            }

            var data = await r.json();

            setScannedData(data.scannedData || null);
            setCards(data.cards?.items ?? []);

            if (data.cards?.items?.length > 0) {
                setMessage(ICO_OK + " " + data.message);
                setMsgType("success");
            } else {
                setMessage(ICO_WARN + " " + (data.message || "No cards found."));
                setMsgType("warning");
            }
        } catch (err) {
            console.error(err);
            setMessage(ICO_FAIL + " Error scanning: " + err.message);
            setMsgType("error");
        } finally {
            setScanning(false);
        }
    }, [API_URL]);

    // ?? Capture from live camera ??
    var captureAndScan = useCallback(async function() {
        if (!videoRef.current || !canvasRef.current) return;

        var video = videoRef.current;
        var canvas = canvasRef.current;
        canvas.width = video.videoWidth;
        canvas.height = video.videoHeight;

        var ctx = canvas.getContext("2d");
        ctx.drawImage(video, 0, 0, canvas.width, canvas.height);

        var dataUrl = canvas.toDataURL("image/png");
        setCapturedImage(dataUrl);
        stopCamera();
        sendToScan(dataUrl);
    }, [stopCamera, sendToScan]);

    // ?? Upload from file / gallery ??
    var handleFileUpload = useCallback(function(e) {
        var file = e.target.files && e.target.files[0];
        if (!file) return;

        var reader = new FileReader();
        reader.onload = function(ev) {
            var dataUrl = ev.target.result;
            setCapturedImage(dataUrl);
            stopCamera();
            sendToScan(dataUrl);
        };
        reader.readAsDataURL(file);

        // Reset input so the same file can be selected again
        e.target.value = "";
    }, [stopCamera, sendToScan]);

    var reset = function() {
        setCapturedImage(null);
        setCards([]);
        setScannedData(null);
        setMessage("");
        setMsgType("");
    };

    var msgClass = msgType === "success" ? "msg-success"
                 : msgType === "error"   ? "msg-error"
                 : "msg-warning";

    return (
        <div className="page-content-v2">
            <h1 className="section-title">{ICO_CAM} Card Scanner</h1>

            {/* Controls */}
            <div className="glass-panel" style={{ marginBottom: 20, textAlign: "center" }}>
                <p style={{ color: "var(--text-muted)", marginBottom: 12 }}>
                    Use your phone camera or upload a photo of a Pok{"\u00e9"}mon card.
                    {" "}The scanner will read <strong>all visible data</strong> from it.
                </p>
                <div style={{ display: "flex", gap: 10, justifyContent: "center", flexWrap: "wrap" }}>
                    {!cameraActive && !capturedImage && (
                        <>
                            <button className="btn btn-gold" onClick={function() { startCamera(); }}>
                                {ICO_CAM} Live Camera
                            </button>
                            <button className="btn btn-blue" onClick={function() { fileInputRef.current?.click(); }}>
                                {ICO_UPLOAD} Upload Photo
                            </button>
                        </>
                    )}
                    {cameraActive && (
                        <>
                            <button className="btn btn-green" onClick={captureAndScan} disabled={scanning}>
                                {scanning ? "Scanning\u2026" : ICO_SCAN + " Capture & Scan"}
                            </button>
                            <button className="btn btn-outline btn-sm" onClick={flipCamera} title="Switch front/back camera">
                                {ICO_FLIP} Flip
                            </button>
                            <button className="btn btn-outline btn-sm" onClick={stopCamera}>
                                {ICO_STOP} Stop
                            </button>
                        </>
                    )}
                    {capturedImage && !cameraActive && (
                        <>
                            <button className="btn btn-gold" onClick={function() { reset(); startCamera(); }}>
                                {ICO_CAM} Scan Another (Camera)
                            </button>
                            <button className="btn btn-blue" onClick={function() { reset(); fileInputRef.current?.click(); }}>
                                {ICO_UPLOAD} Upload Another
                            </button>
                            <button className="btn btn-outline btn-sm" onClick={reset}>
                                Clear
                            </button>
                        </>
                    )}
                </div>

                {/* Hidden file input — accept images, camera capture on mobile */}
                <input
                    ref={fileInputRef}
                    type="file"
                    accept="image/*"
                    capture="environment"
                    style={{ display: "none" }}
                    onChange={handleFileUpload}
                />
            </div>

            {/* Message */}
            {message && (
                <div className={msgClass} style={{ marginBottom: 16, textAlign: "center" }}>
                    {message}
                </div>
            )}

            {/* Camera + Scanned Data */}
            <div style={{
                display: "grid",
                gridTemplateColumns: scannedData ? "1fr 1fr" : "1fr",
                gap: 20,
                marginBottom: 20,
                alignItems: "start",
            }}>
                {/* Camera / Captured Image */}
                <div style={{ display: "flex", justifyContent: "center" }}>
                    <div style={{
                        position: "relative",
                        width: "100%",
                        maxWidth: 640,
                        display: (cameraActive || capturedImage) ? "block" : "none",
                    }}>
                        {/* Live video — always in DOM for stable ref */}
                        <video
                            ref={videoRef}
                            autoPlay
                            playsInline
                            muted
                            style={{
                                display: cameraActive ? "block" : "none",
                                width: "100%",
                                borderRadius: "var(--radius)",
                                border: "2px solid var(--border-glow)",
                                background: "#000",
                            }}
                        />

                        {/* Guide overlay on live camera */}
                        {cameraActive && (
                            <div style={{
                                position: "absolute",
                                top: 0, left: 0, right: 0, bottom: 0,
                                pointerEvents: "none",
                                borderRadius: "var(--radius)",
                                overflow: "hidden",
                            }}>
                                {/* Name region */}
                                <div style={{
                                    position: "absolute",
                                    top: "4%", left: "10%", right: "10%", height: "14%",
                                    border: "2px dashed rgba(251,191,36,0.7)",
                                    borderRadius: 8,
                                    display: "flex",
                                    alignItems: "center",
                                    justifyContent: "center",
                                }}>
                                    <span style={{
                                        background: "rgba(0,0,0,0.55)",
                                        color: "var(--gold)",
                                        fontSize: ".65rem",
                                        fontFamily: "var(--font-title)",
                                        padding: "2px 10px",
                                        borderRadius: 4,
                                        letterSpacing: "1px",
                                        textTransform: "uppercase",
                                    }}>
                                        Card name
                                    </span>
                                </div>

                                {/* Full card outline */}
                                <div style={{
                                    position: "absolute",
                                    top: "2%", left: "8%", right: "8%", bottom: "2%",
                                    border: "2px solid rgba(251,191,36,0.3)",
                                    borderRadius: 12,
                                }}>
                                    <span style={{
                                        position: "absolute",
                                        bottom: 8, left: "50%", transform: "translateX(-50%)",
                                        background: "rgba(0,0,0,0.55)",
                                        color: "var(--text-muted)",
                                        fontSize: ".6rem",
                                        fontFamily: "var(--font-title)",
                                        padding: "2px 10px",
                                        borderRadius: 4,
                                        letterSpacing: "1px",
                                        textTransform: "uppercase",
                                        whiteSpace: "nowrap",
                                    }}>
                                        Fit entire card in frame
                                    </span>
                                </div>

                                {/* Corner marks */}
                                {[
                                    { top: "2%", left: "8%" },
                                    { top: "2%", right: "8%" },
                                    { bottom: "2%", left: "8%" },
                                    { bottom: "2%", right: "8%" },
                                ].map(function(pos, i) {
                                    return (
                                        <div key={i} style={{
                                            position: "absolute",
                                            ...pos,
                                            width: 24, height: 24,
                                            borderTop: (pos.top != null) ? "3px solid var(--gold)" : "none",
                                            borderBottom: (pos.bottom != null) ? "3px solid var(--gold)" : "none",
                                            borderLeft: (pos.left != null) ? "3px solid var(--gold)" : "none",
                                            borderRight: (pos.right != null) ? "3px solid var(--gold)" : "none",
                                            opacity: 0.7,
                                        }} />
                                    );
                                })}
                            </div>
                        )}

                        {/* Captured snapshot */}
                        {capturedImage && !cameraActive && (
                            <img
                                src={capturedImage}
                                alt="Captured card"
                                style={{
                                    width: "100%",
                                    borderRadius: "var(--radius)",
                                    border: "2px solid var(--border-glow)",
                                }}
                            />
                        )}
                    </div>
                </div>

                {/* Scanned Data Panel */}
                {scannedData && (
                    <div className="glass-panel" style={{ padding: 20 }}>
                        <h3 style={{
                            fontFamily: "var(--font-title)",
                            fontSize: ".9rem",
                            color: "var(--gold)",
                            letterSpacing: "1px",
                            textTransform: "uppercase",
                            marginBottom: 14,
                            borderBottom: "1px solid var(--border)",
                            paddingBottom: 8,
                        }}>
                            {ICO_SCAN} Extracted Data
                        </h3>

                        <div style={{ display: "flex", flexDirection: "column", gap: 2 }}>
                            <DataRow label="Name" value={scannedData.name} />
                            <DataRow label="HP" value={scannedData.hp} />
                            <DataRow label="Supertype" value={scannedData.supertype} />
                            <DataRow label="Stage" value={scannedData.stage} />
                            <DataRow label="Evolves From" value={scannedData.evolvesFrom} />
                            <DataRow label="Attacks" value={scannedData.attacks} />
                            <DataRow label="Weakness" value={scannedData.weakness} />
                            <DataRow label="Resistance" value={scannedData.resistance} />
                            <DataRow label="Retreat" value={scannedData.retreatCost} />
                            <DataRow label="Artist" value={scannedData.artist} />
                            <DataRow label="Number" value={scannedData.number} />
                        </div>

                        {scannedData.fullText && (
                            <details style={{ marginTop: 14 }}>
                                <summary style={{
                                    cursor: "pointer",
                                    color: "var(--text-dim)",
                                    fontSize: ".72rem",
                                    fontFamily: "var(--font-title)",
                                    letterSpacing: ".5px",
                                    textTransform: "uppercase",
                                }}>
                                    Raw OCR Text
                                </summary>
                                <pre style={{
                                    marginTop: 8,
                                    padding: 12,
                                    background: "rgba(0,0,0,.3)",
                                    borderRadius: 8,
                                    color: "var(--text-muted)",
                                    fontSize: ".72rem",
                                    lineHeight: 1.5,
                                    whiteSpace: "pre-wrap",
                                    wordBreak: "break-word",
                                    maxHeight: 200,
                                    overflowY: "auto",
                                }}>
                                    {scannedData.fullText}
                                </pre>
                            </details>
                        )}
                    </div>
                )}
            </div>

            <canvas ref={canvasRef} style={{ display: "none" }} />

            {/* Results Grid */}
            {cards.length > 0 && (
                <>
                    <h2 className="section-title" style={{ fontSize: "1rem" }}>Matching Cards in Database</h2>
                    <div className="card-grid-v2">
                        {cards.map(function(card) {
                            return (
                                <Link key={card.cardId} to={"/card/cardid/" + card.cardId}>
                                    <div className="card-thumb">
                                        <img src={card.imageLarge} alt={card.name} />
                                    </div>
                                </Link>
                            );
                        })}
                    </div>
                </>
            )}

            {/* Loading overlay */}
            {scanning && (
                <div className="loading-screen" style={{ background: "rgba(10,14,26,.8)" }}>
                    <div className="pokeball-spinner" />
                    <span className="loading-text">Reading card data\u2026</span>
                </div>
            )}
        </div>
    );
}
