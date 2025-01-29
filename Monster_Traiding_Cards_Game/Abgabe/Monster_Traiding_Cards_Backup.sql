--
-- PostgreSQL database dump
--

-- Dumped from database version 17.2 (Debian 17.2-1.pgdg120+1)
-- Dumped by pg_dump version 17.1

-- Started on 2025-01-29 04:17:42

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- TOC entry 4 (class 2615 OID 2200)
-- Name: public; Type: SCHEMA; Schema: -; Owner: pg_database_owner
--

CREATE SCHEMA public;


ALTER SCHEMA public OWNER TO pg_database_owner;

--
-- TOC entry 3462 (class 0 OID 0)
-- Dependencies: 4
-- Name: SCHEMA public; Type: COMMENT; Schema: -; Owner: pg_database_owner
--

COMMENT ON SCHEMA public IS 'standard public schema';


SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- TOC entry 232 (class 1259 OID 41219)
-- Name: battlerounds; Type: TABLE; Schema: public; Owner: kevin
--

CREATE TABLE public.battlerounds (
    id integer NOT NULL,
    battleid integer,
    roundnumber integer NOT NULL,
    player1cardid integer,
    player2cardid integer,
    winnerid integer,
    loserid integer,
    log text
);


ALTER TABLE public.battlerounds OWNER TO kevin;

--
-- TOC entry 231 (class 1259 OID 41218)
-- Name: battlerounds_id_seq; Type: SEQUENCE; Schema: public; Owner: kevin
--

CREATE SEQUENCE public.battlerounds_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.battlerounds_id_seq OWNER TO kevin;

--
-- TOC entry 3463 (class 0 OID 0)
-- Dependencies: 231
-- Name: battlerounds_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: kevin
--

ALTER SEQUENCE public.battlerounds_id_seq OWNED BY public.battlerounds.id;


--
-- TOC entry 230 (class 1259 OID 41192)
-- Name: battles; Type: TABLE; Schema: public; Owner: kevin
--

CREATE TABLE public.battles (
    id integer NOT NULL,
    user1id integer,
    user2id integer,
    winnerid integer,
    loserid integer
);


ALTER TABLE public.battles OWNER TO kevin;

--
-- TOC entry 229 (class 1259 OID 41191)
-- Name: battles_id_seq; Type: SEQUENCE; Schema: public; Owner: kevin
--

CREATE SEQUENCE public.battles_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.battles_id_seq OWNER TO kevin;

--
-- TOC entry 3464 (class 0 OID 0)
-- Dependencies: 229
-- Name: battles_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: kevin
--

ALTER SEQUENCE public.battles_id_seq OWNED BY public.battles.id;


--
-- TOC entry 220 (class 1259 OID 41098)
-- Name: cards; Type: TABLE; Schema: public; Owner: kevin
--

CREATE TABLE public.cards (
    id integer NOT NULL,
    name character varying(100) NOT NULL,
    type character varying(50) NOT NULL,
    damage integer NOT NULL,
    elementtype character varying(50) NOT NULL,
    abilities jsonb
);


ALTER TABLE public.cards OWNER TO kevin;

--
-- TOC entry 219 (class 1259 OID 41097)
-- Name: cards_id_seq; Type: SEQUENCE; Schema: public; Owner: kevin
--

CREATE SEQUENCE public.cards_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.cards_id_seq OWNER TO kevin;

--
-- TOC entry 3465 (class 0 OID 0)
-- Dependencies: 219
-- Name: cards_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: kevin
--

ALTER SEQUENCE public.cards_id_seq OWNED BY public.cards.id;


--
-- TOC entry 234 (class 1259 OID 41253)
-- Name: lobby; Type: TABLE; Schema: public; Owner: kevin
--

CREATE TABLE public.lobby (
    id integer NOT NULL,
    userid integer NOT NULL,
    createdat timestamp without time zone DEFAULT CURRENT_TIMESTAMP
);


ALTER TABLE public.lobby OWNER TO kevin;

--
-- TOC entry 233 (class 1259 OID 41252)
-- Name: lobby_id_seq; Type: SEQUENCE; Schema: public; Owner: kevin
--

CREATE SEQUENCE public.lobby_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.lobby_id_seq OWNER TO kevin;

--
-- TOC entry 3466 (class 0 OID 0)
-- Dependencies: 233
-- Name: lobby_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: kevin
--

ALTER SEQUENCE public.lobby_id_seq OWNED BY public.lobby.id;


--
-- TOC entry 226 (class 1259 OID 41143)
-- Name: packages; Type: TABLE; Schema: public; Owner: kevin
--

CREATE TABLE public.packages (
    id integer NOT NULL,
    card1id integer,
    card2id integer,
    card3id integer,
    card4id integer,
    card5id integer
);


ALTER TABLE public.packages OWNER TO kevin;

--
-- TOC entry 225 (class 1259 OID 41142)
-- Name: packages_id_seq; Type: SEQUENCE; Schema: public; Owner: kevin
--

CREATE SEQUENCE public.packages_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.packages_id_seq OWNER TO kevin;

--
-- TOC entry 3467 (class 0 OID 0)
-- Dependencies: 225
-- Name: packages_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: kevin
--

ALTER SEQUENCE public.packages_id_seq OWNED BY public.packages.id;


--
-- TOC entry 228 (class 1259 OID 41175)
-- Name: trades; Type: TABLE; Schema: public; Owner: kevin
--

CREATE TABLE public.trades (
    id integer NOT NULL,
    userid integer,
    offeredcardid integer,
    requestedcardtype character varying(50),
    requestedcardelementtype character varying(50),
    minimumdamage integer
);


ALTER TABLE public.trades OWNER TO kevin;

--
-- TOC entry 227 (class 1259 OID 41174)
-- Name: trades_id_seq; Type: SEQUENCE; Schema: public; Owner: kevin
--

CREATE SEQUENCE public.trades_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.trades_id_seq OWNER TO kevin;

--
-- TOC entry 3468 (class 0 OID 0)
-- Dependencies: 227
-- Name: trades_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: kevin
--

ALTER SEQUENCE public.trades_id_seq OWNED BY public.trades.id;


--
-- TOC entry 224 (class 1259 OID 41126)
-- Name: userdecks; Type: TABLE; Schema: public; Owner: kevin
--

CREATE TABLE public.userdecks (
    id integer NOT NULL,
    userid integer NOT NULL,
    cardid integer NOT NULL
);


ALTER TABLE public.userdecks OWNER TO kevin;

--
-- TOC entry 223 (class 1259 OID 41125)
-- Name: userdecks_id_seq; Type: SEQUENCE; Schema: public; Owner: kevin
--

CREATE SEQUENCE public.userdecks_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.userdecks_id_seq OWNER TO kevin;

--
-- TOC entry 3469 (class 0 OID 0)
-- Dependencies: 223
-- Name: userdecks_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: kevin
--

ALTER SEQUENCE public.userdecks_id_seq OWNED BY public.userdecks.id;


--
-- TOC entry 218 (class 1259 OID 41080)
-- Name: users; Type: TABLE; Schema: public; Owner: kevin
--

CREATE TABLE public.users (
    id integer NOT NULL,
    username character varying(50) NOT NULL,
    password character varying(255) NOT NULL,
    fullname character varying(100),
    email character varying(100),
    coins integer DEFAULT 20,
    elo integer DEFAULT 100,
    wins integer DEFAULT 0,
    losses integer DEFAULT 0,
    totalgames integer DEFAULT 0,
    sessiontoken character varying(255),
    motto character varying(255),
    createdat timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    updatedat timestamp without time zone DEFAULT CURRENT_TIMESTAMP
);


ALTER TABLE public.users OWNER TO kevin;

--
-- TOC entry 217 (class 1259 OID 41079)
-- Name: users_id_seq; Type: SEQUENCE; Schema: public; Owner: kevin
--

CREATE SEQUENCE public.users_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.users_id_seq OWNER TO kevin;

--
-- TOC entry 3470 (class 0 OID 0)
-- Dependencies: 217
-- Name: users_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: kevin
--

ALTER SEQUENCE public.users_id_seq OWNED BY public.users.id;


--
-- TOC entry 222 (class 1259 OID 41109)
-- Name: userstacks; Type: TABLE; Schema: public; Owner: kevin
--

CREATE TABLE public.userstacks (
    id integer NOT NULL,
    userid integer NOT NULL,
    cardid integer NOT NULL
);


ALTER TABLE public.userstacks OWNER TO kevin;

--
-- TOC entry 221 (class 1259 OID 41108)
-- Name: userstacks_id_seq; Type: SEQUENCE; Schema: public; Owner: kevin
--

CREATE SEQUENCE public.userstacks_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.userstacks_id_seq OWNER TO kevin;

--
-- TOC entry 3471 (class 0 OID 0)
-- Dependencies: 221
-- Name: userstacks_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: kevin
--

ALTER SEQUENCE public.userstacks_id_seq OWNED BY public.userstacks.id;


--
-- TOC entry 3264 (class 2604 OID 41222)
-- Name: battlerounds id; Type: DEFAULT; Schema: public; Owner: kevin
--

ALTER TABLE ONLY public.battlerounds ALTER COLUMN id SET DEFAULT nextval('public.battlerounds_id_seq'::regclass);


--
-- TOC entry 3263 (class 2604 OID 41195)
-- Name: battles id; Type: DEFAULT; Schema: public; Owner: kevin
--

ALTER TABLE ONLY public.battles ALTER COLUMN id SET DEFAULT nextval('public.battles_id_seq'::regclass);


--
-- TOC entry 3258 (class 2604 OID 41101)
-- Name: cards id; Type: DEFAULT; Schema: public; Owner: kevin
--

ALTER TABLE ONLY public.cards ALTER COLUMN id SET DEFAULT nextval('public.cards_id_seq'::regclass);


--
-- TOC entry 3265 (class 2604 OID 41256)
-- Name: lobby id; Type: DEFAULT; Schema: public; Owner: kevin
--

ALTER TABLE ONLY public.lobby ALTER COLUMN id SET DEFAULT nextval('public.lobby_id_seq'::regclass);


--
-- TOC entry 3261 (class 2604 OID 41146)
-- Name: packages id; Type: DEFAULT; Schema: public; Owner: kevin
--

ALTER TABLE ONLY public.packages ALTER COLUMN id SET DEFAULT nextval('public.packages_id_seq'::regclass);


--
-- TOC entry 3262 (class 2604 OID 41178)
-- Name: trades id; Type: DEFAULT; Schema: public; Owner: kevin
--

ALTER TABLE ONLY public.trades ALTER COLUMN id SET DEFAULT nextval('public.trades_id_seq'::regclass);


--
-- TOC entry 3260 (class 2604 OID 41129)
-- Name: userdecks id; Type: DEFAULT; Schema: public; Owner: kevin
--

ALTER TABLE ONLY public.userdecks ALTER COLUMN id SET DEFAULT nextval('public.userdecks_id_seq'::regclass);


--
-- TOC entry 3250 (class 2604 OID 41083)
-- Name: users id; Type: DEFAULT; Schema: public; Owner: kevin
--

ALTER TABLE ONLY public.users ALTER COLUMN id SET DEFAULT nextval('public.users_id_seq'::regclass);


--
-- TOC entry 3259 (class 2604 OID 41112)
-- Name: userstacks id; Type: DEFAULT; Schema: public; Owner: kevin
--

ALTER TABLE ONLY public.userstacks ALTER COLUMN id SET DEFAULT nextval('public.userstacks_id_seq'::regclass);


--
-- TOC entry 3286 (class 2606 OID 41226)
-- Name: battlerounds battlerounds_pkey; Type: CONSTRAINT; Schema: public; Owner: kevin
--

ALTER TABLE ONLY public.battlerounds
    ADD CONSTRAINT battlerounds_pkey PRIMARY KEY (id);


--
-- TOC entry 3284 (class 2606 OID 41197)
-- Name: battles battles_pkey; Type: CONSTRAINT; Schema: public; Owner: kevin
--

ALTER TABLE ONLY public.battles
    ADD CONSTRAINT battles_pkey PRIMARY KEY (id);


--
-- TOC entry 3272 (class 2606 OID 41107)
-- Name: cards cards_name_key; Type: CONSTRAINT; Schema: public; Owner: kevin
--

ALTER TABLE ONLY public.cards
    ADD CONSTRAINT cards_name_key UNIQUE (name);


--
-- TOC entry 3274 (class 2606 OID 41105)
-- Name: cards cards_pkey; Type: CONSTRAINT; Schema: public; Owner: kevin
--

ALTER TABLE ONLY public.cards
    ADD CONSTRAINT cards_pkey PRIMARY KEY (id);


--
-- TOC entry 3288 (class 2606 OID 41259)
-- Name: lobby lobby_pkey; Type: CONSTRAINT; Schema: public; Owner: kevin
--

ALTER TABLE ONLY public.lobby
    ADD CONSTRAINT lobby_pkey PRIMARY KEY (id);


--
-- TOC entry 3290 (class 2606 OID 41261)
-- Name: lobby lobby_userid_key; Type: CONSTRAINT; Schema: public; Owner: kevin
--

ALTER TABLE ONLY public.lobby
    ADD CONSTRAINT lobby_userid_key UNIQUE (userid);


--
-- TOC entry 3280 (class 2606 OID 41148)
-- Name: packages packages_pkey; Type: CONSTRAINT; Schema: public; Owner: kevin
--

ALTER TABLE ONLY public.packages
    ADD CONSTRAINT packages_pkey PRIMARY KEY (id);


--
-- TOC entry 3282 (class 2606 OID 41180)
-- Name: trades trades_pkey; Type: CONSTRAINT; Schema: public; Owner: kevin
--

ALTER TABLE ONLY public.trades
    ADD CONSTRAINT trades_pkey PRIMARY KEY (id);


--
-- TOC entry 3278 (class 2606 OID 41131)
-- Name: userdecks userdecks_pkey; Type: CONSTRAINT; Schema: public; Owner: kevin
--

ALTER TABLE ONLY public.userdecks
    ADD CONSTRAINT userdecks_pkey PRIMARY KEY (id);


--
-- TOC entry 3268 (class 2606 OID 41094)
-- Name: users users_pkey; Type: CONSTRAINT; Schema: public; Owner: kevin
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_pkey PRIMARY KEY (id);


--
-- TOC entry 3270 (class 2606 OID 41096)
-- Name: users users_username_key; Type: CONSTRAINT; Schema: public; Owner: kevin
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_username_key UNIQUE (username);


--
-- TOC entry 3276 (class 2606 OID 41114)
-- Name: userstacks userstacks_pkey; Type: CONSTRAINT; Schema: public; Owner: kevin
--

ALTER TABLE ONLY public.userstacks
    ADD CONSTRAINT userstacks_pkey PRIMARY KEY (id);


--
-- TOC entry 3306 (class 2606 OID 41227)
-- Name: battlerounds battlerounds_battleid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: kevin
--

ALTER TABLE ONLY public.battlerounds
    ADD CONSTRAINT battlerounds_battleid_fkey FOREIGN KEY (battleid) REFERENCES public.battles(id);


--
-- TOC entry 3307 (class 2606 OID 41247)
-- Name: battlerounds battlerounds_loserid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: kevin
--

ALTER TABLE ONLY public.battlerounds
    ADD CONSTRAINT battlerounds_loserid_fkey FOREIGN KEY (loserid) REFERENCES public.users(id);


--
-- TOC entry 3308 (class 2606 OID 41232)
-- Name: battlerounds battlerounds_player1cardid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: kevin
--

ALTER TABLE ONLY public.battlerounds
    ADD CONSTRAINT battlerounds_player1cardid_fkey FOREIGN KEY (player1cardid) REFERENCES public.cards(id);


--
-- TOC entry 3309 (class 2606 OID 41237)
-- Name: battlerounds battlerounds_player2cardid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: kevin
--

ALTER TABLE ONLY public.battlerounds
    ADD CONSTRAINT battlerounds_player2cardid_fkey FOREIGN KEY (player2cardid) REFERENCES public.cards(id);


--
-- TOC entry 3310 (class 2606 OID 41242)
-- Name: battlerounds battlerounds_winnerid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: kevin
--

ALTER TABLE ONLY public.battlerounds
    ADD CONSTRAINT battlerounds_winnerid_fkey FOREIGN KEY (winnerid) REFERENCES public.users(id);


--
-- TOC entry 3302 (class 2606 OID 41213)
-- Name: battles battles_loserid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: kevin
--

ALTER TABLE ONLY public.battles
    ADD CONSTRAINT battles_loserid_fkey FOREIGN KEY (loserid) REFERENCES public.users(id);


--
-- TOC entry 3303 (class 2606 OID 41198)
-- Name: battles battles_user1id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: kevin
--

ALTER TABLE ONLY public.battles
    ADD CONSTRAINT battles_user1id_fkey FOREIGN KEY (user1id) REFERENCES public.users(id);


--
-- TOC entry 3304 (class 2606 OID 41203)
-- Name: battles battles_user2id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: kevin
--

ALTER TABLE ONLY public.battles
    ADD CONSTRAINT battles_user2id_fkey FOREIGN KEY (user2id) REFERENCES public.users(id);


--
-- TOC entry 3305 (class 2606 OID 41208)
-- Name: battles battles_winnerid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: kevin
--

ALTER TABLE ONLY public.battles
    ADD CONSTRAINT battles_winnerid_fkey FOREIGN KEY (winnerid) REFERENCES public.users(id);


--
-- TOC entry 3311 (class 2606 OID 41262)
-- Name: lobby lobby_userid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: kevin
--

ALTER TABLE ONLY public.lobby
    ADD CONSTRAINT lobby_userid_fkey FOREIGN KEY (userid) REFERENCES public.users(id);


--
-- TOC entry 3295 (class 2606 OID 41149)
-- Name: packages packages_card1id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: kevin
--

ALTER TABLE ONLY public.packages
    ADD CONSTRAINT packages_card1id_fkey FOREIGN KEY (card1id) REFERENCES public.cards(id);


--
-- TOC entry 3296 (class 2606 OID 41154)
-- Name: packages packages_card2id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: kevin
--

ALTER TABLE ONLY public.packages
    ADD CONSTRAINT packages_card2id_fkey FOREIGN KEY (card2id) REFERENCES public.cards(id);


--
-- TOC entry 3297 (class 2606 OID 41159)
-- Name: packages packages_card3id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: kevin
--

ALTER TABLE ONLY public.packages
    ADD CONSTRAINT packages_card3id_fkey FOREIGN KEY (card3id) REFERENCES public.cards(id);


--
-- TOC entry 3298 (class 2606 OID 41164)
-- Name: packages packages_card4id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: kevin
--

ALTER TABLE ONLY public.packages
    ADD CONSTRAINT packages_card4id_fkey FOREIGN KEY (card4id) REFERENCES public.cards(id);


--
-- TOC entry 3299 (class 2606 OID 41169)
-- Name: packages packages_card5id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: kevin
--

ALTER TABLE ONLY public.packages
    ADD CONSTRAINT packages_card5id_fkey FOREIGN KEY (card5id) REFERENCES public.cards(id);


--
-- TOC entry 3300 (class 2606 OID 41186)
-- Name: trades trades_offeredcardid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: kevin
--

ALTER TABLE ONLY public.trades
    ADD CONSTRAINT trades_offeredcardid_fkey FOREIGN KEY (offeredcardid) REFERENCES public.cards(id);


--
-- TOC entry 3301 (class 2606 OID 41181)
-- Name: trades trades_userid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: kevin
--

ALTER TABLE ONLY public.trades
    ADD CONSTRAINT trades_userid_fkey FOREIGN KEY (userid) REFERENCES public.users(id);


--
-- TOC entry 3293 (class 2606 OID 41137)
-- Name: userdecks userdecks_cardid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: kevin
--

ALTER TABLE ONLY public.userdecks
    ADD CONSTRAINT userdecks_cardid_fkey FOREIGN KEY (cardid) REFERENCES public.cards(id);


--
-- TOC entry 3294 (class 2606 OID 41132)
-- Name: userdecks userdecks_userid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: kevin
--

ALTER TABLE ONLY public.userdecks
    ADD CONSTRAINT userdecks_userid_fkey FOREIGN KEY (userid) REFERENCES public.users(id);


--
-- TOC entry 3291 (class 2606 OID 41120)
-- Name: userstacks userstacks_cardid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: kevin
--

ALTER TABLE ONLY public.userstacks
    ADD CONSTRAINT userstacks_cardid_fkey FOREIGN KEY (cardid) REFERENCES public.cards(id);


--
-- TOC entry 3292 (class 2606 OID 41115)
-- Name: userstacks userstacks_userid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: kevin
--

ALTER TABLE ONLY public.userstacks
    ADD CONSTRAINT userstacks_userid_fkey FOREIGN KEY (userid) REFERENCES public.users(id);


-- Completed on 2025-01-29 04:17:42

--
-- PostgreSQL database dump complete
--

