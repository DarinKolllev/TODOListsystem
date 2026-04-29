(function () {
    const storageKey = "taskflow-theme";
    const toggleBtn = document.getElementById("themeToggle");

    const getPreferredTheme = () => {
        const saved = localStorage.getItem(storageKey);
        if (saved === "light" || saved === "dark") {
            return saved;
        }
        return window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light";
    };

    const applyTheme = (theme) => {
        document.documentElement.setAttribute("data-theme", theme);
        if (toggleBtn) {
            toggleBtn.textContent = theme === "dark" ? "Light mode" : "Dark mode";
        }
    };

    const initialTheme = getPreferredTheme();
    applyTheme(initialTheme);

    if (toggleBtn) {
        toggleBtn.addEventListener("click", () => {
            const current = document.documentElement.getAttribute("data-theme") || "light";
            const next = current === "dark" ? "light" : "dark";
            localStorage.setItem(storageKey, next);
            applyTheme(next);
        });
    }
})();
