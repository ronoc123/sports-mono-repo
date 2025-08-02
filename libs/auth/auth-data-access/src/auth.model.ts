

export type AuthState = {
  loggedIn: boolean;
  user: any;
};

export const initialUserValue: any = {
  email: '',
  username: '',
  password: '',
  bio: '',
  image: '',
};

export const authInitialState: AuthState = {
  loggedIn: false,
  user: initialUserValue,
};