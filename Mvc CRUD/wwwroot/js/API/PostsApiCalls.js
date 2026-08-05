import request from "./GlobalRequest.js";

export const PostsApiRequest =
{
    getPosts: async (pgSize, pageNum) => {
        const res = await request.get(`/Index?pageSize=${pgSize}&pageNumber=${pageNum}`, { headers: { "X-Requested-With": "XMLHttpRequest" } });
        return res.data;
    },

    getComments: async (postId, pgSize, pageNum) => {
        const res = await request.get(`/GetComments?postId=${postId}&pageSize=${pgSize}&pageNumber=${pageNum}`);
        return res.data;
    },

    getCommentReplies: async (commentId, pgSize, pageNum) => {
        const res = await request.get(`/CommentReplies?commentId=${commentId}&pageSize=${pgSize}&pageNumber=${pageNum}`);
        return res.data;
    },

    getRepliesOfReply: async (replyId, pgSize, pageNum) => {
        const res = await request.get(`/Replies?replyId=${replyId}&pageSize=${pgSize}&pageNumber=${pageNum}`);
        return res.data;
    }
    
}
