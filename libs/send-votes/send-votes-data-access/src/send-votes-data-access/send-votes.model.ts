export interface SendVotesUserInfo {
  id: string;
  email: string;
  firstName?: string | null;
  lastName?: string | null;
  fullName: string;
  profilePictureUrl?: string | null;
}

export interface SendVotesCommand {
  UserId: string;
  Amount: number;
  OrganizationId: string;
}
