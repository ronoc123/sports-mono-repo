// Player interfaces matching C# domain models
export interface Player {
  id: string;
  name: string;
  position: string;
  imageUrl: string;
  updatedAt: string;
  age: number;
  organizationId?: string;
  leagueId: string;
  isActive: boolean;
  isVeteran: boolean;
  isYoungPlayer: boolean;
  marketValue: number;
  createdAt: string;
  teamName?: string; // Optional team name for display purposes
}

export interface PlayerOption {
  id: string;
  title: string;
  description: string;
  votes: number;
  createdAt: string;
  expiresAt: string;
  playerId: string;
  organizationId: string;
  isActive: boolean;
  isExpired: boolean;
  isPopular: boolean;
  isTrending: boolean;
  timeRemaining: number; // in milliseconds
  daysRemaining: number;
  popularityLevel: string;
  engagementScore: number;
  shouldPromote: boolean;
  player?: Player; // Optional populated player data
}

export enum PlayerPosition {
  QB = "QB",
  WR = "WR",
  TE = "TE",
  RB = "RB",
  DT = "DT",
  DE = "DE",
  LB = "LB",
  CB = "CB",
  S = "S",
}

const defaultDate = new Date().toISOString();
const defaultAge = 25;
const defaultLeagueId = "nfl-league-id";
const defaultOrganizationId = "dallas-cowboys-id";

// Helper function to create complete player objects
const createPlayer = (
  id: string,
  name: string,
  position: PlayerPosition,
  imageUrl: string,
  teamName: string = "Dallas Cowboys"
): Player => ({
  id,
  name,
  position,
  imageUrl,
  teamName,
  updatedAt: defaultDate,
  age: defaultAge,
  leagueId: defaultLeagueId,
  organizationId: defaultOrganizationId,
  isActive: true,
  isVeteran: false,
  isYoungPlayer: false,
  marketValue: 1000000,
  createdAt: defaultDate,
});

export const OFFENSE_PLAYERS: Player[] = [
  createPlayer(
    "1",
    "Dak Prescott",
    PlayerPosition.QB,
    "/assets/players/dak-prescott.png"
  ),
  createPlayer(
    "2",
    "CeeDee Lamb",
    PlayerPosition.WR,
    "/assets/players/ceedee-lamb.png"
  ),
  createPlayer(
    "3",
    "Brandin Cooks",
    PlayerPosition.WR,
    "/assets/players/brandin-cooks.png"
  ),
  createPlayer(
    "4",
    "Jake Ferguson",
    PlayerPosition.TE,
    "/assets/players/jake-ferguson.png"
  ),
  createPlayer(
    "5",
    "Tony Pollard",
    PlayerPosition.RB,
    "/assets/players/tony-pollard.png"
  ),
  createPlayer(
    "6",
    "Osa Odighizuwa",
    PlayerPosition.DT,
    "/assets/players/osa-odighizuwa.png"
  ),
];

export const DEFENSE_PLAYERS: Player[] = [
  createPlayer(
    "7",
    "Micah Parsons",
    PlayerPosition.LB,
    "/assets/players/micah-parsons.png"
  ),
  createPlayer(
    "8",
    "DeMarcus Lawrence",
    PlayerPosition.DE,
    "/assets/players/demarcus-lawrence.png"
  ),
  createPlayer(
    "9",
    "Johnathan Hankins",
    PlayerPosition.DT,
    "/assets/players/johnathan-hankins.png"
  ),
  createPlayer(
    "10",
    "Leighton Vander Esch",
    PlayerPosition.LB,
    "/assets/players/leighton-vander-esch.png"
  ),
  createPlayer(
    "11",
    "Trevon Diggs",
    PlayerPosition.CB,
    "/assets/players/trevon-diggs.png"
  ),
  createPlayer(
    "12",
    "Donovan Wilson",
    PlayerPosition.S,
    "/assets/players/donovan-wilson.png"
  ),
];
