import axios from "https://cdn.jsdelivr.net/npm/axios@1.6.8/+esm";

const request = axios.create({
    baseURL: "/Home",
    timeout: 30000,
    headers: {
        "Content-Type": "application/json"
    }
});

request.interceptors.response.use(
    function (response) {
        console.log("✅ Response:", response.status, response.config.url);
        return response;
    },
    function (error) {
        const status = error.response?.status;
        if (error.code === "ECONNABORTED") toastr.error("⏱ Request timeout");
        if (!error.response && !error.code === "ECONNABORTED") alert("🌐 Network error");

        switch (status) {
            case 401:
                alert("🔐 Unauthorized - redirecting to login");
                window.location.href = "/Home";
                break;
            case 403:
                alert("🚫 Forbidden");
                break;
            case 404:
                alert("❓ Not found");
                break;
            case 500:
                alert("🔥 Server error");
                break;
        }

        return Promise.reject(error);
    }
);

export default request;