import request from "./GlobalRequest.js";

export const friendRequestApi = {

    getAllPotentialFriends: async (pgSize, pageNum) => {
        const res = await request.get(`/friendRequests?pageSize=${pgSize}&pageNumber=${pageNum}`, { headers: { "X-Requested-With": "XMLHttpRequest" } });
        return res.data
    },

    getRecievedRequest: async (pgSize, pageNum) => {
        const res = await request.get(`/RecievedFriendRequest?pageSize=${pgSize}&pageNumber=${pageNum}`);
        return res.data;
    },

    getSentFriendRequest: async (pgSize, pageNum) => {
        const res = await request.get(`/GetAllSentRequest?pageSize=${pgSize}&pageNumber=${pageNum}`);
        return res.data;
    },

    sendfriendRequest: async (userid, username, lastname, profilePic, email) => {
        const res = await request.post(`/SendFriendRequest?ToUserId=${encodeURIComponent(userid)}&ToUserName=${encodeURIComponent(username)}&ToUserEmail=${encodeURIComponent(email)}&ToUser_LastName=${encodeURIComponent(lastname)}&ToUser_ProfilePicUrl=${encodeURIComponent(profilePic)}`);
        return res.data;
    },

    acceptFriendRequest: async (userid, username) => {
        const res = await request.post(`/AcceptRequest?FriendId=${userid}&FriendName=${username}`);
        return res.data;
    },

    rejectRecievedRequest: async (userid) => {
        const res = await request.put(`/RejectRequest?friendUserId=${userid}`);
        return res.data;
    },

    cancelSentRequest: async (userid) => {
        const res = await request.put(`/RejectRequest?friendUserId=${userid}`);
        return res.data;
    },

    removeUser: async (userid, username) => {
        const res = await request.post(`/BlockUser?BlockUserId=${encodeURIComponent(userid)}&BlockUserName=${encodeURIComponent(username)}`);
        return res.data;
    }

};
