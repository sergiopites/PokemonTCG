import React, { useEffect, useState } from "react";
import { Link } from "react-router-dom";

export default function SearchCard() {
    const API_URL = import.meta.env.VITE_API_URL || "";

    const [availableRarities, setAvailableRarities] = useState([]);
    const [availableSubTypes, setAvailableSubTypes] = useState([]);
    const [availableTypes, setAvailableTypes] = useState([]);
    const [availableSuperTypes, setAvailableSuperTypes] = useState([]);
    const [availableSets, setAvailableSets] = useState([]);

    const [cards, setCards] = useState([]);
    const [page, setPage] = useState(1);
    const [totalPages, setTotalPages] = useState(1);
    const [search, setSearch] = useState("");
    const [number, setNumber] = useState("");
    const [setId, setSetId] = useState("");
    const [supertype, setSuperType] = useState("");
    const [type, setType] = useState("");
    const [subtype, setSubType] = useState("");
    const [rarity, setRarity] = useState("");

    var norm = function(arr, keys) {
        keys = keys || [];
        if (!Array.isArray(arr)) return [];
        return arr.map(function(item) {
            if (item == null) return "";
            if (typeof item === "string") return item;
            for (var ki = 0; ki < keys.length; ki++) { var k = keys[ki]; if (item[k] && typeof item[k] === "string") return item[k]; }
            var vals = Object.values(item);
            for (var vi = 0; vi < vals.length; vi++) { var v = vals[vi]; if (typeof v === "string" || typeof v === "number") return String(v); }
            return JSON.stringify(item);
        });
    };

    useEffect(function() {
        fetch(API_URL + "/api/card/filters")
            .then(function(r) { return r.json(); })
            .then(function(d) {
                setAvailableRarities(norm(d?.rarities ?? d?.Rarities ?? [], ["rarity", "name", "value"]));
                setAvailableSubTypes(norm(d?.subtypes ?? d?.SubTypes ?? [], ["subtype", "name", "value"]));
                setAvailableTypes(norm(d?.types ?? d?.Types ?? [], ["type", "name", "value"]));
                setAvailableSuperTypes(norm(d?.supertypes ?? d?.superTypes ?? d?.SuperTypes ?? [], ["supertype", "name", "value"]));
            })
            .catch(function(e) { console.error("Error loading filters:", e); });
    }, [API_URL]);

    useEffect(function() {
        fetch(API_URL + "/api/set/all")
            .then(function(r) { return r.json(); })
            .then(function(d) { setAvailableSets(Array.isArray(d) ? d : []); })
            .catch(console.error);
    }, [API_URL]);

    useEffect(function() {
        var params = new URLSearchParams();
        if (search) params.append("name", search);
        if (number) params.append("number", number);
        if (setId) params.append("setId", setId);
        if (subtype) params.append("subtype", subtype);
        if (type) params.append("type", type);
        if (supertype) params.append("supertype", supertype);
        if (rarity) params.append("rarity", rarity);
        params.append("page", String(page));
        params.append("pageSize", "55");

        fetch(API_URL + "/api/card/search?" + params)
            .then(function(r) { return r.json(); })
            .then(function(d) {
                setCards(d?.items ?? []);
                var total = Number(d?.totalCount ?? 0);
                var size = Number(d?.pageSize ?? 55);
                setTotalPages(size > 0 ? Math.max(1, Math.ceil(total / size)) : 1);
            })
            .catch(console.error);
    }, [search, number, setId, supertype, type, subtype, rarity, page, API_URL]);

    var field = function(label, value, setter, placeholder) {
        return (
            <div className="field-group">
                <label className="field-label">{label}</label>
                <input
                    className="field-input"
                    type="text"
                    placeholder={placeholder}
                    value={value}
                    onChange={function(e) { setPage(1); setter(e.target.value); }}
                />
            </div>
        );
    };

    var sel = function(label, value, setter, options, allLabel) {
        return (
            <div className="field-group">
                <label className="field-label">{label}</label>
                <select className="field-select" value={value} onChange={function(e) { setPage(1); setter(e.target.value); }}>
                    <option value="">{allLabel}</option>
                    {options.map(function(o, i) {
                        var val = typeof o === "object" ? (o.setId ?? o.id ?? i) : o;
                        var txt = typeof o === "object" ? (o.name ?? String(o.setId)) : o;
                        return <option key={i} value={val}>{txt}</option>;
                    })}
                </select>
            </div>
        );
    };

    return (
        <div className="page-content-v2">
            <h1 className="section-title">Search Cards</h1>

            <div className="glass-panel" style={{ marginBottom: 24 }}>
                <div className="filter-grid">
                    {field("Number", number, setNumber, "e.g. 25")}
                    {field("Name", search, setSearch, "e.g. Charizard")}
                    {sel("Set", setId, setSetId, availableSets, "All Sets")}
                    {sel("Super Type", supertype, setSuperType, availableSuperTypes, "All")}
                    {sel("Type", type, setType, availableTypes, "All")}
                    {sel("Sub Type", subtype, setSubType, availableSubTypes, "All")}
                    {sel("Rarity", rarity, setRarity, availableRarities, "All")}
                </div>
            </div>

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

            <div className="pagination-bar">
                <button className="btn btn-outline btn-xs" disabled={page <= 1} onClick={function() { setPage(function(p) { return p - 1; }); }}>
                    {"\u25C0"} Prev
                </button>
                <span>
                    Page <strong>{page}</strong> of <strong>{totalPages}</strong>
                </span>
                <button className="btn btn-outline btn-xs" disabled={page >= totalPages} onClick={function() { setPage(function(p) { return p + 1; }); }}>
                    Next {"\u25B6"}
                </button>
            </div>
        </div>
    );
}
