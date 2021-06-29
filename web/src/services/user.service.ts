import ApiService from "./api.service";
import User from "@/models/User";
import IUserList from "@/models/IUserList";

const UserService = {
  async me(): Promise<User> {
    const response = await ApiService.get(`/.auth/me`);
    const user = new User(response.data.clientPrincipal);
    return user;
  },

  async lists(userName: string): Promise<Array<IUserList>> {
    const response = await ApiService.get(`/api/links/user/${userName}`);
    return response ? <Array<IUserList>>response.data : [];
  }
};

export default UserService;
