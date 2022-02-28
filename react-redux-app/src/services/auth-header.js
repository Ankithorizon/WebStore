export default function authHeader() {
    const token = JSON.parse(localStorage.getItem('token'));

    if (token) {
        return { Authorization: 'Bearer ' + token }; 
        // return { 'x-access-token': user.accessToken }; 
    } else {
        return {};
    }
}