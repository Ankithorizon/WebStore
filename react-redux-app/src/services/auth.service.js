import http from "../auth-http-common";

class AuthService {
  
    register = async (data, myRole) => {
        return await http.post(`/register/${myRole}`, data);
    }


    login = async (data) => {
        return await http.post(`/login`, data);
    }

    logout() {
        localStorage.removeItem("token");
        localStorage.removeItem("user-name");
        localStorage.removeItem("user-role");
        localStorage.removeItem("my-cart");
        localStorage.removeItem("cart-total");
        console.log('logging out!');
    }

    getCurrentUser() {
        return JSON.parse(localStorage.getItem('user-name'));
    }

    getCurrentUserRole() {
        return JSON.parse(localStorage.getItem('user-role'));
    }
}
export default new AuthService();