(function () {
    var storageKey = "checkflow-theme";
    var root = document.documentElement;
    var toggles = Array.prototype.slice.call(document.querySelectorAll("[data-theme-toggle]"));
    var toggleLabels = Array.prototype.slice.call(document.querySelectorAll("[data-theme-label]"));
    var dateTargets = Array.prototype.slice.call(document.querySelectorAll("[data-current-date]"));
    var navToggle = document.querySelector("[data-nav-toggle]");
    var navPanel = document.querySelector("[data-nav-panel]");
    var userMenus = Array.prototype.slice.call(document.querySelectorAll("[data-user-menu]"));

    function readTheme() {
        try {
            return localStorage.getItem(storageKey) || root.getAttribute("data-theme") || "light";
        } catch (error) {
            return root.getAttribute("data-theme") || "light";
        }
    }

    function writeTheme(theme) {
        root.setAttribute("data-theme", theme);

        toggles.forEach(function (toggle) {
            if (toggle instanceof HTMLButtonElement) {
                toggle.setAttribute("aria-pressed", (theme === "dark").toString());
            }
        });

        toggleLabels.forEach(function (toggleLabel) {
            if (toggleLabel instanceof HTMLElement) {
                toggleLabel.textContent = theme === "dark" ? "Modo claro" : "Modo escuro";
            }
        });

        try {
            localStorage.setItem(storageKey, theme);
        } catch (error) {
        }
    }

    function formatCurrentDate() {
        var formatted = new Intl.DateTimeFormat("pt-BR", {
            weekday: "long",
            day: "2-digit",
            month: "long",
            year: "numeric"
        }).format(new Date());
        var label = formatted.replace(/\b\p{L}/gu, function (letter) {
            return letter.toUpperCase();
        });

        dateTargets.forEach(function (dateTarget) {
            if (dateTarget instanceof HTMLElement) {
                dateTarget.textContent = label;
            }
        });
    }

    writeTheme(readTheme());
    formatCurrentDate();

    toggles.forEach(function (toggle) {
        if (!(toggle instanceof HTMLButtonElement)) {
            return;
        }

        toggle.addEventListener("click", function () {
            var currentTheme = root.getAttribute("data-theme") === "dark" ? "dark" : "light";
            writeTheme(currentTheme === "dark" ? "light" : "dark");
        });
    });

    if (navToggle instanceof HTMLButtonElement && navPanel instanceof HTMLElement) {
        navToggle.addEventListener("click", function () {
            var isOpen = navPanel.classList.toggle("is-open");
            navToggle.setAttribute("aria-expanded", isOpen.toString());
        });

        window.addEventListener("resize", function () {
            if (window.innerWidth > 980) {
                navPanel.classList.remove("is-open");
                navToggle.setAttribute("aria-expanded", "false");
            }
        });
    }

    if (userMenus.length > 0) {
        function closeAllUserMenus(exceptMenu) {
            userMenus.forEach(function (menu) {
                if (!(menu instanceof HTMLElement) || menu === exceptMenu) {
                    return;
                }

                var toggle = menu.querySelector("[data-user-menu-toggle]");
                var panel = menu.querySelector("[data-user-menu-panel]");

                if (panel instanceof HTMLElement) {
                    panel.classList.remove("is-open");
                }

                if (toggle instanceof HTMLButtonElement) {
                    toggle.setAttribute("aria-expanded", "false");
                }
            });
        }

        userMenus.forEach(function (menu) {
            if (!(menu instanceof HTMLElement)) {
                return;
            }

            var toggle = menu.querySelector("[data-user-menu-toggle]");
            var panel = menu.querySelector("[data-user-menu-panel]");

            if (!(toggle instanceof HTMLButtonElement) || !(panel instanceof HTMLElement)) {
                return;
            }

            toggle.addEventListener("click", function (event) {
                event.stopPropagation();
                var willOpen = !panel.classList.contains("is-open");
                closeAllUserMenus(menu);
                panel.classList.toggle("is-open", willOpen);
                toggle.setAttribute("aria-expanded", willOpen.toString());
            });

            panel.addEventListener("click", function (event) {
                event.stopPropagation();
            });
        });

        document.addEventListener("click", function () {
            closeAllUserMenus(null);
        });

        document.addEventListener("keydown", function (event) {
            if (event.key === "Escape") {
                closeAllUserMenus(null);
            }
        });
    }
})();
