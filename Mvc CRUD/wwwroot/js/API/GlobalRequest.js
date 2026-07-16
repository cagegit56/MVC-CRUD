import axios from "https://cdn.jsdelivr.net/npm/axios@1.6.8/+esm";

const request = axios.create({
    baseURL: "/Home",
    timeout: 20000,
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
        if (error.code === "ECONNABORTED") toastr.error("Connection Timeout, slow internet connection!");
        if (!error.response && !error.code === "ECONNABORTED") toastr.error("Network error");

        switch (status) {
            case 401:
                toastr.error("Unauthorized - redirecting to login");
                window.location.href = "/Home";
                break;
            case 403:
                toastr.error("🚫 Forbidden");
                break;
            case 404:
                toastr.error("❓ Not found");
                break;
            case 500:
                toastr.error("🔥 Server error");
                break;
        }

        return Promise.reject(error);
    }
);

toastr.options = {
    "closeButton": true,
    "progressBar": false,
    "positionClass": "toast-top-center",
    "timeOut": "20000"
};

export default request;