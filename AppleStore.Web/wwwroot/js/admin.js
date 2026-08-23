document.getElementById('admin-menu')?.addEventListener('click', () => document.querySelector('.admin-sidebar')?.classList.toggle('open'));

const initAdminTooltips = (scope = document) => {
    if (!window.bootstrap?.Tooltip) return;

    scope.querySelectorAll('[data-bs-toggle="tooltip"]').forEach(element => {
        if (bootstrap.Tooltip.getInstance(element)) return;
        new bootstrap.Tooltip(element);
    });
};

const initConfirmForms = (scope = document) => {
    scope.querySelectorAll('.js-confirm-form').forEach(form => {
        if (form.dataset.confirmReady === 'true') return;
        form.dataset.confirmReady = 'true';

        form.addEventListener('submit', async event => {
            if (!window.Swal) return;

            event.preventDefault();
            const root = getComputedStyle(document.documentElement);
            const result = await Swal.fire({
                title: form.dataset.confirmTitle || 'Xác nhận thao tác',
                text: form.dataset.confirmText || 'Bạn có chắc muốn tiếp tục?',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: 'Xác nhận',
                cancelButtonText: 'Hủy',
                confirmButtonColor: root.getPropertyValue('--color-primary').trim(),
                cancelButtonColor: root.getPropertyValue('--color-text-secondary').trim(),
                customClass: { popup: 'apple-modal' }
            });

            if (result.isConfirmed) form.submit();
        });
    });
};

const cleanSearchParams = params => {
    Array.from(params.entries()).forEach(([key, value]) => {
        if (value === null || value === '') params.delete(key);
    });
    return params;
};

const updateExportLink = form => {
    const exportLink = document.querySelector('[data-admin-filter-export]');
    if (!exportLink) return;

    const formData = new FormData(form);
    const exportUrl = new URL(exportLink.dataset.exportBase || exportLink.href, window.location.origin);
    const params = new URLSearchParams();

    ['search', 'status'].forEach(key => {
        const value = formData.get(key);
        if (value) params.set(key, value);
    });

    exportUrl.search = params.toString();
    exportLink.href = exportUrl.pathname + exportUrl.search;
};

const initAdminLiveFilters = () => {
    document.querySelectorAll('[data-admin-live-filter]').forEach(form => {
        if (form.dataset.liveFilterReady === 'true') return;
        form.dataset.liveFilterReady = 'true';

        const target = document.querySelector(form.dataset.resultsTarget);
        if (!target) return;

        const debounceDelay = Number(form.dataset.debounce || 400);
        let debounceTimer = 0;
        let controller;

        const buildUrl = () => {
            const url = new URL(form.action || window.location.href, window.location.origin);
            const params = cleanSearchParams(new URLSearchParams(new FormData(form)));
            url.search = params.toString();
            return url;
        };

        const loadResults = async requestedUrl => {
            const url = requestedUrl instanceof URL ? requestedUrl : new URL(requestedUrl || buildUrl(), window.location.origin);

            controller?.abort();
            const activeController = new AbortController();
            controller = activeController;
            target.classList.add('is-loading');

            try {
                const response = await fetch(url, {
                    headers: { 'X-Requested-With': 'XMLHttpRequest' },
                    signal: activeController.signal
                });

                if (!response.ok) throw new Error('Không thể tải dữ liệu khách hàng.');

                target.innerHTML = await response.text();
                window.history.pushState({}, '', url.pathname + url.search);
                updateExportLink(form);
                initAdminTooltips(target);
                initConfirmForms(target);
            } catch (error) {
                if (error.name !== 'AbortError') {
                    window.location.href = url.pathname + url.search;
                }
            } finally {
                if (!activeController.signal.aborted) target.classList.remove('is-loading');
            }
        };

        form.addEventListener('submit', event => {
            event.preventDefault();
            window.clearTimeout(debounceTimer);
            loadResults();
        });

        form.querySelectorAll('[data-admin-live-search]').forEach(input => {
            input.addEventListener('input', () => {
                window.clearTimeout(debounceTimer);
                debounceTimer = window.setTimeout(() => loadResults(), debounceDelay);
            });
        });

        target.addEventListener('click', event => {
            const link = event.target.closest('.pagination a.page-link');
            if (!link) return;

            event.preventDefault();
            loadResults(link.href);
        });
    });
};

initAdminTooltips();
initConfirmForms();
initAdminLiveFilters();

let variantIndex = document.querySelectorAll('#variants-container .variant-admin-row').length;
document.getElementById('add-variant')?.addEventListener('click', () => {
    const container = document.getElementById('variants-container');
    if (!container) return;

    const row = document.createElement('div');
    row.className = 'variant-admin-row';
    row.innerHTML = `<input type="hidden" name="Variants[${variantIndex}].Id" value="0"/><input class="form-control" name="Variants[${variantIndex}].Color" placeholder="Màu sắc" required/><input class="form-control" name="Variants[${variantIndex}].Storage" placeholder="Dung lượng" required/><input class="form-control" name="Variants[${variantIndex}].SKU" placeholder="SKU" required/><input class="form-control" name="Variants[${variantIndex}].Price" type="number" min="1" placeholder="Giá" required/><input class="form-control" name="Variants[${variantIndex}].StockQuantity" type="number" min="0" placeholder="Tồn kho" required/><button type="button" class="btn btn-light remove-variant" title="Xóa phiên bản"><i class="bi bi-x-lg"></i></button>`;
    container.appendChild(row);
    variantIndex++;
});

document.addEventListener('click', event => {
    const button = event.target.closest('.remove-variant');
    if (button) button.closest('.variant-admin-row')?.remove();
});

document.querySelector('input[name="Image"]')?.addEventListener('change', event => {
    const file = event.target.files?.[0];
    const preview = document.getElementById('product-image-preview');
    if (!file || !preview) return;

    preview.src = URL.createObjectURL(file);
});
