import { PlayerOption } from "@sports-ui/api-types";

export type PlayerOptionState = {
  playerOptions: PlayerOption[];
  loading: boolean;
  error: string | null;
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
};

export const playerOptionInitialState: PlayerOptionState = {
  playerOptions: [],
  loading: false,
  error: null,
  totalCount: 0,
  pageNumber: 1,
  pageSize: 10,
  totalPages: 0,
};
