(() => {
    const rootSelector = '[data-ant-dropdown]';
    const itemSelector = '.ant-dropdown-item:not(.is-disabled):not(:disabled)';

    const getOpenDropdowns = () => Array.from(document.querySelectorAll(`${rootSelector}.is-open`));

    const getItems = dropdown => Array.from(dropdown.querySelectorAll(itemSelector));

    const closeDropdown = dropdown => {
        if (!dropdown?.classList.contains('is-open')) return;

        dropdown.classList.remove('is-open', 'is-align-end', 'is-up');
        dropdown.querySelector('.ant-dropdown-trigger')?.setAttribute('aria-expanded', 'false');
        getItems(dropdown).forEach(item => item.classList.remove('is-active'));
    };

    const closeAll = except => {
        getOpenDropdowns().forEach(dropdown => {
            if (dropdown !== except) closeDropdown(dropdown);
        });
    };

    const positionDropdown = dropdown => {
        const panel = dropdown.querySelector('.ant-dropdown-panel');
        if (!panel) return;

        dropdown.classList.remove('is-align-end', 'is-up');

        const rect = dropdown.getBoundingClientRect();
        const panelWidth = Math.max(panel.offsetWidth || rect.width, rect.width);
        const panelHeight = panel.offsetHeight || 220;
        const viewportWidth = window.innerWidth || document.documentElement.clientWidth;
        const viewportHeight = window.innerHeight || document.documentElement.clientHeight;

        if (rect.left + panelWidth > viewportWidth - 12) {
            dropdown.classList.add('is-align-end');
        }

        if (rect.bottom + panelHeight + 12 > viewportHeight && rect.top > panelHeight + 12) {
            dropdown.classList.add('is-up');
        }
    };

    const openDropdown = dropdown => {
        closeAll(dropdown);
        dropdown.classList.add('is-open');
        dropdown.querySelector('.ant-dropdown-trigger')?.setAttribute('aria-expanded', 'true');
        positionDropdown(dropdown);
    };

    const toggleDropdown = dropdown => {
        if (dropdown.classList.contains('is-open')) {
            closeDropdown(dropdown);
            return;
        }

        openDropdown(dropdown);
    };

    const setActiveItem = (dropdown, nextIndex) => {
        const items = getItems(dropdown);
        if (!items.length) return;

        const safeIndex = (nextIndex + items.length) % items.length;
        items.forEach(item => item.classList.remove('is-active'));
        items[safeIndex].classList.add('is-active');
        items[safeIndex].focus({ preventScroll: true });
    };

    const selectItem = (dropdown, item) => {
        if (!item || item.classList.contains('is-disabled') || item.disabled) return;

        const value = item.dataset.value ?? '';
        const label = item.dataset.label ?? item.textContent.trim();
        const labelTarget = dropdown.querySelector('[data-ant-dropdown-label]');
        const inputSelector = dropdown.dataset.input;
        const formSelector = dropdown.dataset.submitForm;

        dropdown.querySelectorAll('.ant-dropdown-item.is-selected').forEach(selected => {
            selected.classList.remove('is-selected');
            selected.setAttribute('aria-selected', 'false');
        });

        item.classList.add('is-selected');
        item.setAttribute('aria-selected', 'true');

        if (labelTarget) labelTarget.textContent = label;

        if (inputSelector) {
            const input = document.querySelector(inputSelector);
            if (input) {
                input.value = value;
                input.dispatchEvent(new Event('change', { bubbles: true }));
            }
        }

        closeDropdown(dropdown);

        if (formSelector) {
            document.querySelector(formSelector)?.requestSubmit();
        }
    };

    const initDropdown = dropdown => {
        if (dropdown.dataset.antDropdownReady === 'true') return;

        const trigger = dropdown.querySelector('.ant-dropdown-trigger');
        const panel = dropdown.querySelector('.ant-dropdown-panel');
        if (!trigger || !panel) return;

        dropdown.dataset.antDropdownReady = 'true';
        trigger.setAttribute('aria-expanded', 'false');

        trigger.addEventListener('click', event => {
            event.preventDefault();
            toggleDropdown(dropdown);
        });

        trigger.addEventListener('keydown', event => {
            if (!['ArrowDown', 'ArrowUp', 'Enter', ' '].includes(event.key)) return;
            event.preventDefault();
            openDropdown(dropdown);

            const items = getItems(dropdown);
            const selectedIndex = items.findIndex(item => item.classList.contains('is-selected'));
            const fallbackIndex = event.key === 'ArrowUp' ? items.length - 1 : 0;
            setActiveItem(dropdown, selectedIndex >= 0 ? selectedIndex : fallbackIndex);
        });

        getItems(dropdown).forEach((item, index) => {
            item.setAttribute('tabindex', '-1');

            item.addEventListener('click', event => {
                event.preventDefault();
                selectItem(dropdown, item);
            });

            item.addEventListener('keydown', event => {
                const items = getItems(dropdown);
                const currentIndex = items.indexOf(item);

                if (event.key === 'ArrowDown') {
                    event.preventDefault();
                    setActiveItem(dropdown, currentIndex + 1);
                }

                if (event.key === 'ArrowUp') {
                    event.preventDefault();
                    setActiveItem(dropdown, currentIndex - 1);
                }

                if (event.key === 'Enter' || event.key === ' ') {
                    event.preventDefault();
                    selectItem(dropdown, item);
                }

                if (event.key === 'Escape') {
                    event.preventDefault();
                    closeDropdown(dropdown);
                    trigger.focus({ preventScroll: true });
                }
            });

            if (index === 0 && !dropdown.querySelector('.ant-dropdown-item.is-selected')) {
                item.classList.add('is-selected');
                item.setAttribute('aria-selected', 'true');
            }
        });
    };

    document.addEventListener('click', event => {
        if (event.target.closest(rootSelector)) return;
        closeAll();
    });

    document.addEventListener('keydown', event => {
        if (event.key !== 'Escape') return;
        getOpenDropdowns().forEach(dropdown => closeDropdown(dropdown));
    });

    window.addEventListener('resize', () => {
        getOpenDropdowns().forEach(positionDropdown);
    });

    document.addEventListener('DOMContentLoaded', () => {
        document.querySelectorAll(rootSelector).forEach(initDropdown);
    });

    window.AppleStoreDropdown = {
        init: initDropdown,
        closeAll
    };
})();
