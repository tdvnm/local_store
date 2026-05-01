import { browser } from "$app/environment";

export type User = {
  id: number;
  name: string;
  flat_no: string;
  phone: string;
};

let user = $state<User | null>(null);

if (browser) {
  const saved = localStorage.getItem("lucky_user");
  if (saved) {
    try {
      user = JSON.parse(saved);
    } catch {
      /* corrupted */
    }
  }
}

export const auth = {
  get user() {
    return user;
  },
  get loggedIn() {
    return user !== null;
  },
  login(u: User) {
    user = u;
    if (browser) localStorage.setItem("lucky_user", JSON.stringify(u));
  },
  logout() {
    user = null;
    if (browser) {
      localStorage.removeItem("lucky_user");
      localStorage.removeItem("lucky_cart");
    }
  },
};
