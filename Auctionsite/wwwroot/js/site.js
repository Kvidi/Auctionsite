
document.addEventListener("DOMContentLoaded", function () {
    console.log("DOM Content Loaded - Initializing all functionality");

    // Initialize search functionality
    initializeSearchFunctionality();

    // Initialize persistent filter display
    initializePersistentFilters();

    // Initialize all filters modal functionality
    initializeAllFiltersModal();

    // Initialize quick filter bar functionality
    initializeQuickFilterBar();

    // Initialize existing carousel functionality (if present)
    initializeCarouselAndThumbnails();

    // Initialize existing category dropdown functionality (if present)
    initializeEnhancedCategoryDropdown();
});

// ====================================================================================================================
//                                          SEARCH FUNCTIONALITY                                                       
// ====================================================================================================================

function initializeSearchFunctionality() {
    const searchInput = document.getElementById("search-input");
    const searchForm = document.getElementById("search-form");

    if (searchInput && searchForm) {
        searchInput.addEventListener("keypress", function (event) {
            if (event.key === "Enter") {
                event.preventDefault(); // Prevent default form submission on Enter
                searchForm.submit();
            }
        });
    }
}

// ====================================================================================================================
//                                          ALL FILTERS MODAL JAVASCRIPT                                              
// ====================================================================================================================

function initializeAllFiltersModal() {
    console.log("Initializing All Filters Modal");

    const applyFiltersBtn = document.getElementById("apply-all-filters");
    const allFiltersForm = document.getElementById("all-filters-form");
    const modal = document.getElementById("allFiltersModal");

    if (!applyFiltersBtn) {
        console.warn("Apply filters button not found with ID: apply-all-filters");
        return;
    }

    if (!allFiltersForm) {
        console.warn("All filters form not found with ID: all-filters-form");
        return;
    }

    console.log("All filters modal elements found successfully");

    // Handle apply filters button click
    applyFiltersBtn.addEventListener("click", function (e) {
        console.log("Apply filters button clicked");
        e.preventDefault();

        // Ensure the form is valid before submission
        if (allFiltersForm.checkValidity()) {
            console.log("Form is valid, submitting...");
            allFiltersForm.submit();
        } else {
            console.warn("Form validation failed");
            allFiltersForm.reportValidity();
        }
    });

    // Handle modal show event - sync current filters to modal
    if (modal) {
        modal.addEventListener("show.bs.modal", function () {
            console.log("Modal is being shown, syncing filters");
            syncFiltersToModal();
        });
    }

    // Handle category radio button changes in modal
    const categoryRadios = document.querySelectorAll("#allFiltersModal input[name=\"categoryId\"]");
    categoryRadios.forEach(radio => {
        radio.addEventListener("change", function () {
            if (this.checked) {
                console.log("Category selected in modal:", this.value);
            }
        });
    });

    // Handle seller type radio button changes
    const sellerTypeRadios = document.querySelectorAll("#allFiltersModal input[name=\"sellerType\"]");
    sellerTypeRadios.forEach(radio => {
        radio.addEventListener("change", function () {
            if (this.checked) {
                console.log("Seller type selected in modal:", this.value);
            }
        });
    });

    // Handle ad type radio button changes
    const adTypeRadios = document.querySelectorAll("#allFiltersModal input[name=\"adType\"]");
    adTypeRadios.forEach(radio => {
        radio.addEventListener("change", function () {
            if (this.checked) {
                console.log("Ad type selected in modal:", this.value);
            }
        });
    });

    // Handle condition checkboxes
    const conditionCheckboxes = document.querySelectorAll("#allFiltersModal input[name=\"selectedConditions\"]");
    conditionCheckboxes.forEach(checkbox => {
        checkbox.addEventListener("change", function () {
            console.log("Condition changed in modal:", this.value, this.checked);
        });
    });

    // Handle price inputs
    const minPriceInput = document.querySelector("#allFiltersModal input[name=\"minPrice\"]");
    const maxPriceInput = document.querySelector("#allFiltersModal input[name=\"maxPrice\"]");

    if (minPriceInput) {
        minPriceInput.addEventListener("input", function () {
            console.log("Min price changed in modal:", this.value);
        });
    }

    if (maxPriceInput) {
        maxPriceInput.addEventListener("input", function () {
            console.log("Max price changed in modal:", this.value);
        });
    }
}

function syncFiltersToModal() {
    // This function syncs the current page filters to the modal
    // It's called when the modal is opened

    const urlParams = new URLSearchParams(window.location.search);

    // Sync category selection
    const categoryId = urlParams.get("CategoryId") || urlParams.get("categoryId");
    if (categoryId) {
        const categoryRadio = document.querySelector(`#allFiltersModal input[name="categoryId"][value="${categoryId}"]`);
        if (categoryRadio) {
            categoryRadio.checked = true;
        }
    }

    // Sync price range
    const minPrice = urlParams.get("minPrice");
    const maxPrice = urlParams.get("maxPrice");

    if (minPrice) {
        const minPriceInput = document.querySelector("#allFiltersModal input[name=\"minPrice\"]");
        if (minPriceInput) {
            minPriceInput.value = minPrice;
        }
    }

    if (maxPrice) {
        const maxPriceInput = document.querySelector("#allFiltersModal input[name=\"maxPrice\"]");
        if (maxPriceInput) {
            maxPriceInput.value = maxPrice;
        }
    }

    // Sync ad type
    const adType = urlParams.get("adType");
    if (adType) {
        const adTypeRadio = document.querySelector(`#allFiltersModal input[name="adType"][value="${adType}"]`);
        if (adTypeRadio) {
            adTypeRadio.checked = true;
        }
    }

    // Sync seller type
    const sellerType = urlParams.get("sellerType");
    if (sellerType) {
        const sellerTypeRadio = document.querySelector(`#allFiltersModal input[name="sellerType"][value="${sellerType}"]`);
        if (sellerTypeRadio) {
            sellerTypeRadio.checked = true;
        }
    }

    // Sync conditions
    const conditionCheckboxes = document.querySelectorAll("#allFiltersModal input[name=\"selectedConditions\"]");
    conditionCheckboxes.forEach(checkbox => {
        checkbox.checked = false; // Uncheck all first
    });
    const conditions = urlParams.getAll("selectedConditions");
    conditions.forEach(condition => {
        const conditionCheckbox = document.querySelector(`#allFiltersModal input[name="selectedConditions"][value="${condition}"]`);
        if (conditionCheckbox) {
            conditionCheckbox.checked = true;
        }
    });
}

// ====================================================================================================================
//                                          QUICK FILTER BAR FUNCTIONALITY                                           
// ====================================================================================================================

function initializeQuickFilterBar() {
    console.log("Initializing Quick Filter Bar");

    const filterForm = document.getElementById("filter-form");
    if (!filterForm) {
        console.warn("Filter form not found with ID: filter-form");
        return;
    }

    console.log("Filter form found, setting up event listeners");

    // Handle category filter changes in the quick filter dropdown
    const categoryFilters = document.querySelectorAll(".category-filter");
    categoryFilters.forEach(filter => {
        filter.addEventListener("change", function () {
            if (this.checked) {
                console.log("Quick category filter selected:", this.value);
                filterForm.submit();
            }
        });
    });

    // Handle ad type filter changes
    const adTypeFilters = document.querySelectorAll("input[name=\"adType\"]");
    adTypeFilters.forEach(filter => {
        filter.addEventListener("change", function () {
            if (this.checked) {
                console.log("Quick ad type filter selected:", this.value);
                filterForm.submit();
            }
        });
    });

    // Handle condition filter changes
    const conditionFilters = document.querySelectorAll("input[name=\"selectedConditions\"]");
    conditionFilters.forEach(filter => {
        filter.addEventListener("change", function () {
            console.log("Quick condition filter changed:", this.value, this.checked);
            // Auto-submit on condition change
            setTimeout(() => filterForm.submit(), 100); // Small delay to allow multiple selections
        });
    });

    // Handle seller type filter changes
    const sellerTypeFilters = document.querySelectorAll("input[name=\"sellerType\"]");
    sellerTypeFilters.forEach(filter => {
        filter.addEventListener("change", function () {
            if (this.checked) {
                console.log("Quick seller type filter selected:", this.value);
                filterForm.submit();
            }
        });
    });

    // Handle price filter changes (on blur for better UX)
    const minPriceInput = document.querySelector("input[name=\"minPrice\"]");
    const maxPriceInput = document.querySelector("input[name=\"maxPrice\"]");

    if (minPriceInput) {
        minPriceInput.addEventListener("blur", function () {
            if (this.value !== this.defaultValue) {
                console.log("Quick min price filter changed:", this.value);
                filterForm.submit();
            }
        });
    }

    if (maxPriceInput) {
        maxPriceInput.addEventListener("blur", function () {
            if (this.value !== this.defaultValue) {
                console.log("Quick max price filter changed:", this.value);
                filterForm.submit();
            }
        });
    }

    // Handle the main "Tillämpa filter" button
    const applyFilterBtn = filterForm.querySelector("button[type=\"submit\"]");
    if (applyFilterBtn) {
        applyFilterBtn.addEventListener("click", function (e) {
            console.log("Quick filter apply button clicked");
            // Let the form submit naturally
        });
    }
}

// ====================================================================================================================
//                                          PERSISTENT FILTER DISPLAY                                                 
// ====================================================================================================================

function initializePersistentFilters() {
    console.log("Initializing Persistent Filters");

    // Initialize the persistent filter display functionality
    const activeFiltersContainer = document.getElementById("active-filters-container");
    const activeFiltersList = document.getElementById("active-filters-list");
    const clearAllFiltersBtn = document.getElementById("clear-all-filters");

    if (!activeFiltersContainer || !activeFiltersList) {
        console.warn("Persistent filter elements not found");
        return;
    }

    console.log("Persistent filter elements found");

    // Display active filters based on URL parameters
    displayActiveFilters();

    // Handle clear all filters button
    if (clearAllFiltersBtn) {
        clearAllFiltersBtn.addEventListener("click", function () {
            console.log("Clear all filters clicked");
            // Redirect to the same page without any filter parameters
            const baseUrl = window.location.pathname;
            const urlParams = new URLSearchParams(window.location.search);

            // Keep only the search term and search type if they exist
            const searchTerm = urlParams.get("SearchTerm");
            const searchType = urlParams.get("SearchType");

            let newUrl = baseUrl;
            const params = new URLSearchParams();

            if (searchTerm) {
                params.append("SearchTerm", searchTerm);
            }
            if (searchType) {
                params.append("SearchType", searchType);
            }

            if (params.toString()) {
                newUrl += "?" + params.toString();
            }

            window.location.href = newUrl;
        });
    }

    // Handle individual filter removal
    activeFiltersList.addEventListener("click", function (e) {
        if (e.target.classList.contains("remove-filter") || e.target.closest(".remove-filter")) {
            e.preventDefault();
            const filterTag = e.target.closest(".filter-tag");
            if (filterTag && filterTag.href) {
                console.log("Individual filter removal clicked");
                window.location.href = filterTag.href;
            }
        }
    });
}

function displayActiveFilters() {
    const activeFiltersContainer = document.getElementById("active-filters-container");
    const activeFiltersList = document.getElementById("active-filters-list");

    if (!activeFiltersContainer || !activeFiltersList) {
        return;
    }

    const urlParams = new URLSearchParams(window.location.search);
    const filters = [];

    // Search term filter
    const searchTerm = urlParams.get("SearchTerm");
    const searchType = urlParams.get("SearchType");
    if (searchTerm) {
        const searchTypeText = getSearchTypeText(searchType);
        filters.push({
            type: "search-term",
            label: `Sökning (${searchTypeText}): "${searchTerm}"`,
            removeUrl: createRemoveFilterUrl(["SearchTerm", "SearchType"])
        });
    }

    // Category filter
    const categoryId = urlParams.get("CategoryId") || urlParams.get("categoryId");
    if (categoryId) {
        const categoryText = getCategoryText(categoryId);
        filters.push({
            type: "category",
            label: `Kategori: ${categoryText}`,
            removeUrl: createRemoveFilterUrl(["CategoryId", "categoryId"])
        });
    }

    // Price filter
    const minPrice = urlParams.get("minPrice");
    const maxPrice = urlParams.get("maxPrice");
    if (minPrice || maxPrice) {
        const priceText = `${minPrice || "0"} - ${maxPrice || "∞"} kr`;
        filters.push({
            type: "price",
            label: `Pris: ${priceText}`,
            removeUrl: createRemoveFilterUrl(["minPrice", "maxPrice"])
        });
    }

    // Ad type filter
    const adType = urlParams.get("adType");
    if (adType) {
        const adTypeText = getAdTypeText(adType);
        filters.push({
            type: "ad-type",
            label: `Annonsformat: ${adTypeText}`,
            removeUrl: createRemoveFilterUrl(["adType"])
        });
    }

    // Seller type filter
    const sellerType = urlParams.get("sellerType");
    if (sellerType) {
        const sellerTypeText = getSellerTypeText(sellerType);
        filters.push({
            type: "seller-type",
            label: `Säljare: ${sellerTypeText}`,
            removeUrl: createRemoveFilterUrl(["sellerType"])
        });
    }

    // Condition filters
    const conditions = urlParams.getAll("selectedConditions");
    if (conditions.length > 0) {
        conditions.forEach(condition => {
            const conditionText = getConditionText(condition);
            filters.push({
                type: "condition",
                label: `Skick: ${conditionText}`,
                removeUrl: createRemoveFilterUrl(["selectedConditions"], condition)
            });
        });
    }

    // Display filters
    if (filters.length > 0) {
        activeFiltersList.innerHTML = filters.map(filter => `
            <a href="${filter.removeUrl}" class="filter-tag ${filter.type}">
                ${filter.label}
                <button type="button" class="remove-filter" title="Ta bort filter">
                    ×
                </button>
            </a>
        `).join("");

        activeFiltersContainer.style.display = "block";
    } else {
        activeFiltersContainer.style.display = "none";
    }
}

function createRemoveFilterUrl(paramsToRemove, specificValue = null) {
    const urlParams = new URLSearchParams(window.location.search);

    if (specificValue) {
        // Remove specific value from multi-value parameter
        const values = urlParams.getAll(paramsToRemove[0]);
        const filteredValues = values.filter(v => v !== specificValue);
        urlParams.delete(paramsToRemove[0]);
        filteredValues.forEach(v => urlParams.append(paramsToRemove[0], v));
    } else {
        // Remove all specified parameters
        paramsToRemove.forEach(param => urlParams.delete(param));
    }

    const newSearch = urlParams.toString();
    return window.location.pathname + (newSearch ? "?" + newSearch : "");
}

function getSearchTypeText(searchType) {
    switch (searchType) {
        case "ExactPhrase": return "Exakt fras";
        case "StartsWith": return "Börjar med";
        case "Contains": return "Innehåller";
        default: return "Exakt fras";
    }
}

function getCategoryText(categoryId) {
    // This would ideally get the category name from the page data
    // For now, return the ID or a placeholder
    // You would need to populate a map or object with category IDs and names from your backend
    const categoryMap = {
        "1": "Elektronik",
        "2": "Fordon",
        "11": "Mobiltelefoner",
        "12": "Datorer",
        "13": "TV & Audio",
        "21": "Bilar",
        "22": "Motorcyklar"
    };
    return categoryMap[categoryId] || `Kategori ${categoryId}`;
}

function getAdTypeText(adType) {
    switch (adType) {
        case "Auction": return "Auktion";
        case "BuyNow": return "Köp nu";
        case "Both": return "Båda";
        default: return adType;
    }
}

function getSellerTypeText(sellerType) {
    switch (sellerType) {
        case "PrivateOnly": return "Privat";
        case "CompanyOnly": return "Företag";
        default: return sellerType;
    }
}

function getConditionText(condition) {
    switch (condition) {
        case "New": return "Ny";
        case "VeryGood": return "Mycket bra";
        case "Good": return "Bra";
        case "Acceptable": return "Acceptabel";
        case "Poor": return "Dålig";
        default: return condition;
    }
}

// ====================================================================================================================
//                                          CAROUSEL AND THUMBNAILS INTEGRATION                                      
// ====================================================================================================================

let thumbClickHandler = null;
let carouselSlideHandler = null;

function initializeCarouselAndThumbnails(parentContainer) {
    if (!parentContainer) {
        // If no specific container is provided, look for carousels in the entire document
        const carousels = document.querySelectorAll('#adCarousel');
        carousels.forEach(carousel => {
            const container = carousel.closest('.ad-card') || carousel.closest('.container') || document.body;
            initializeCarouselAndThumbnails(container);
        });
        return;
    }

    // Ensure the carousel and thumbnail container exist before proceeding
    const carouselEl = parentContainer.querySelector('#adCarousel');
    if (!carouselEl) return;

    const thumbContainer = parentContainer.querySelector('.thumbnail-list');
    if (!thumbContainer) return;

    // Initialize the Bootstrap carousel instance
    const carousel = bootstrap.Carousel.getOrCreateInstance(carouselEl);

    // Remove any existing event listeners to avoid duplicates
    if (thumbContainer && thumbClickHandler) {
        thumbContainer.removeEventListener('click', thumbClickHandler);
    }

    if (carouselEl && carouselSlideHandler) {
        carouselEl.removeEventListener('slid.bs.carousel', carouselSlideHandler);
    }

    // Create new event handlers for thumbnails and carousel slides
    thumbClickHandler = function (e) {
        // Check if the clicked element is a thumbnail button or its child (img/icon)
        const btn = e.target.closest('.thumbnail-btn');
        if (!btn) return;

        e.preventDefault(); // Prevent default anchor behavior
        e.stopPropagation(); // Stop propagation to avoid triggering other click events

        const idx = Number(btn.dataset.index);
        if (isNaN(idx)) return;

        // Get current active slide index
        const currentSlide = carouselEl.querySelector('.carousel-item.active');
        const allSlides = Array.from(carouselEl.querySelectorAll('.carousel-item'));
        const currentIndex = allSlides.indexOf(currentSlide);

        // Only change slide if we're clicking on a different thumbnail
        if (currentIndex !== idx) {
            carousel.to(idx);
        }

        // Always update active thumbnail (in case it got out of sync)
        updateActiveThumbnail(idx);
    };

    // Create a handler for when the carousel slides
    carouselSlideHandler = function (e) {
        const current = e.to;
        updateActiveThumbnail(current);
    };

    // Helper function to update active thumbnail
    function updateActiveThumbnail(activeIndex) {
        thumbContainer.querySelectorAll('.thumbnail-btn').forEach(btn => {
            const btnIndex = Number(btn.dataset.index);
            if (btnIndex === activeIndex) {
                btn.classList.add('active');
            } else {
                btn.classList.remove('active');
            }
        });
    }

    // Add the new event listeners
    if (thumbContainer) {
        thumbContainer.addEventListener('click', thumbClickHandler);
    }
    carouselEl.addEventListener('slid.bs.carousel', carouselSlideHandler);

    // Ensure the first thumbnail is active if none are set
    // This should match the first carousel item
    const activeThumbnail = thumbContainer.querySelector('.thumbnail-btn.active');
    if (!activeThumbnail) {
        const firstThumbnail = thumbContainer.querySelector('.thumbnail-btn');
        if (firstThumbnail) {
            firstThumbnail.classList.add('active');
        }
    }

    // Also ensure carousel and thumbnails are in sync on initialization
    const activeSlide = carouselEl.querySelector('.carousel-item.active');
    if (activeSlide) {
        const allSlides = Array.from(carouselEl.querySelectorAll('.carousel-item'));
        const activeSlideIndex = allSlides.indexOf(activeSlide);
        updateActiveThumbnail(activeSlideIndex);
    }
}

// ====================================================================================================================
//                                          ENHANCED CATEGORY DROPDOWN                                                
// ====================================================================================================================


function initializeEnhancedCategoryDropdown() {
    // Stack of { id, name, hasChildren }, root = {id:null,name:null}
    const stack = [{ id: null, name: null, hasChildren: true }];
    // Bootstrap elements
    const btn = document.getElementById('categoryBtn');
    const menu = document.getElementById('categoryMenu');
    const hiddenInput = document.getElementById('categoryIdHidden');
    if (!btn || !menu || !hiddenInput) {
        return;
    }

    // Initialize Bootstrap dropdown
    const bsDropdown = new bootstrap.Dropdown(btn);

    // Load initial categories from the hidden input (if any)
    let initialCrumbs = [];
    const rawPath = hiddenInput.dataset.path;
    if (rawPath) {
        try {
            initialCrumbs = JSON.parse(rawPath);
            initialCrumbs.forEach(crumb => {
                const isLeaf = crumb.id === parseInt(hiddenInput.value, 10);
                stack.push({
                    id: crumb.id,
                    name: crumb.name,
                    hasChildren: !isLeaf
                });
            });
        } catch (e) {
            console.error("Could not parse category-path", e);
        }
    }
            
    // Update button text 
    function updateButtonLabel() {
        const top = stack[stack.length - 1];
        btn.textContent = top.name || '-- Välj kategori --';
    }
    updateButtonLabel();

    // Helper function to get the icon class based on category name (only headcategories)
    function getCategoryIcon(name) {
        if (name.includes("Sport")) return "bicycle";
        if (name.includes("Biljetter")) return "ticket";
        if (name.includes("Telefoni")) return "phone";
        if (name.includes("Övrigt")) return "folder";
        return "folder"; // default icon
    }

    // Update button text from the last crumb in categoryPath
    function updateButtonLabel() {
        const top = stack[stack.length - 1];
        btn.textContent = top.name || '-- Välj kategori --';
    }

    function isMenuOpen() {
        return menu.classList.contains('show');
    }

    // ENHANCED: Preserve search term when category is selected
    function getSearchFormData() {
        const searchInput = document.getElementById('search-input') || document.getElementById('unified-search-input');
        const searchTypeDropdown = document.getElementById('search-type-dropdown');
        const searchTerm = searchInput ? searchInput.value.trim() : '';
        const searchType = searchTypeDropdown ? searchTypeDropdown.value : 'ExactPhrase';
        return { searchTerm, searchType };
    }

    function submitSearchWithCategory(categoryId) {
        const { searchTerm, searchType } = getSearchFormData();

        // Build URL with search term, search type, and category
        const params = new URLSearchParams();
        if (searchTerm) {
            params.set('SearchTerm', searchTerm);
            params.set('SearchType', searchType);
        }
        if (categoryId) {
            params.set('CategoryId', categoryId);
        }

        // Navigate to search results with all parameters
        const searchUrl = '/Advertisement/Search' + (params.toString() ? '?' + params.toString() : '');
        window.location.href = searchUrl;
    }

    // Fetch and render subcategories into the dropdown-menu
    function loadCategories(parentId) {
        return fetch(`/Advertisement/GetSubCategories?parentId=${parentId ?? ''}`)
            .then(response => response.json())
            .then(data => {
                // Clear current menu items
                menu.innerHTML = '';

                // If not at root, add a "Back" item
                if (stack.length > 1) {
                    const liBack = document.createElement('li');
                    liBack.innerHTML = `
                        <a class="dropdown-item" href="#" data-action="back">
                          ← Tillbaka
                        </a>`;
                    menu.append(liBack);
                }

                // Are we on the top level?
                const isTopLevel = (parentId == null);

                // Populate with new categories
                data.forEach(cat => {
                    const icon = getCategoryIcon(cat.name);
                    const li = document.createElement('li');
                    // Only include icon for top-level categories
                    const iconHtml = isTopLevel ? `<i class="bi bi-${icon} me-2"></i>` : '';
                    li.innerHTML = `
                        <a class="dropdown-item d-flex align-items-center" 
                           href="#" 
                           data-id="${cat.id}"
                           data-name="${cat.name}"
                           data-has-children="${cat.hasChildren}">
                           ${iconHtml}${cat.name}
                        </a>`;
                    menu.append(li);
                });

                // Attach click handlers to all items
                menu.querySelectorAll('a.dropdown-item').forEach(a => {
                    a.addEventListener('click', e => {
                        e.preventDefault();

                        if (a.dataset.action === 'back') {
                            // Pop all trailing leaf‑selects (hasChildren===false)
                            while (stack.length > 1 && stack[stack.length - 1].hasChildren === false) {
                                stack.pop();
                            }
                            // Pop the current category to jump to its parent
                            if (stack.length > 1) {
                                stack.pop();
                            }
                            // New level to load:
                            const top = stack[stack.length - 1];
                            // Update hidden input (or clear if root)
                            hiddenInput.value = top.id ?? '';
                            // Update button label
                            updateButtonLabel();
                            // Reload previous level
                            loadCategories(top.id).then(() => {
                                bsDropdown.show(); // keep dropdown open                                
                            });
                            return;
                        }

                        // If we are not going back, pop all trailing leaf‑selects
                        if (stack.length > 1 && stack[stack.length - 1].hasChildren === false) {
                            stack.pop();
                        }

                        // Get the selected category data
                        const id = parseInt(a.dataset.id, 10);
                        const name = a.dataset.name;
                        const hasChildren = (a.dataset.hasChildren === 'true');
                        
                        // Push this selection on stack
                        stack.push({ id, name, hasChildren }); 
                        hiddenInput.value = id;
                        updateButtonLabel();

                        if (hasChildren) {
                            // Load next level
                            loadCategories(id).then(() => {
                                bsDropdown.show(); // keep dropdown open                                
                            });
                        } else {
                            bsDropdown.hide(); // close dropdown

                            // ENHANCED: Submit search with category and preserve search term and type
                            submitSearchWithCategory(id);
                        }
                    });
                });
            })
            .catch(err => console.error('Failed to load categories:', err));
    }

    // On first click, load root categories and open dropdown
    btn.addEventListener('click', () => {
        if (!isMenuOpen()) {
            if (stack.length === 1) {
                loadCategories(null);
            }
            bsDropdown.show(); // Open
        } else {
            bsDropdown.hide();// Close
        }
    });

    // Close dropdown when clicking outside
    document.addEventListener('click', function (e) {
        const withinButton = btn.contains(e.target);
        const withinMenu = menu.contains(e.target);
        if (isMenuOpen() && !withinButton && !withinMenu) {
            bsDropdown.hide();
        }
    });

    // ENHANCED: Initialize from URL parameters to show selected category
    function initializeFromUrlParams() {
        const urlParams = new URLSearchParams(window.location.search);
        const categoryId = urlParams.get('CategoryId');

        if (categoryId) {
            // You might want to implement logic here to reconstruct the category path
            // and update the button label accordingly
            // This would require additional API calls to get the category hierarchy
            hiddenInput.value = categoryId;

            // For now, just update the button to show that a category is selected
            btn.textContent = 'Kategori vald'; // You can enhance this to show the actual category name
        }
    }

    // Init button label and URL state
    updateButtonLabel();
    initializeFromUrlParams();
}
//============== CONDITION AND AD TYPE DROPDOWNS ==============

document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('.custom-dropdown').forEach(drop => {
        // Bootstrap elements
        const btn = drop.querySelector('button.dropdown-toggle');
        const menu = drop.querySelector('.dropdown-menu');
        const hidden = drop.querySelector('input[type="hidden"]');

        // Initialize Bootstrap dropdown
        const bsDrop = new bootstrap.Dropdown(btn);

        // Store the Bootstrap instance on the dropdown element
        drop._bsDrop = bsDrop;

        // Keep the initial value when doing edit or after post-back with validation errors
        const initialVal = hidden.value;
        if (initialVal) {
            const selected = menu.querySelector(`a.dropdown-item[data-value="${initialVal}"]`);
            if (selected) btn.textContent = selected.dataset.label;
        }
        
        // Handle item choice
        menu.querySelectorAll('a.dropdown-item').forEach(a => {
            a.addEventListener('click', e => {
                e.preventDefault();
                const value = a.dataset.value;
                const label = a.dataset.label;
                // Button text
                btn.textContent = label;
                // Update hidden input
                hidden.value = value;                
                // Close dropdown
                bsDrop.hide();                
            });
        });
        // Toggle dropdown
        btn.addEventListener('click', () => {
            bsDrop.toggle();
        });        
    });

    // Close all dropdowns when clicking outside
    document.addEventListener('click', e => {
        document.querySelectorAll('.custom-dropdown').forEach(drop => {
            const btn = drop.querySelector('button.dropdown-toggle');
            const menu = drop.querySelector('.dropdown-menu');
            const bs = drop._bsDrop;
            if (!btn.contains(e.target) && !menu.contains(e.target)) {
                bs.hide();
            }
        });
    });    
});


//============== APPROVE PENDING ADVERTISMENT PREVIEW ==============

// ------- Modal function to show the preview of an advertisement --------
document.addEventListener('DOMContentLoaded', () => {
    // Get the approve and reject hidden input fields
    const approveInput = document.getElementById("approveAdId");
    const rejectInput = document.getElementById("rejectAdId");

    // Attach click event to all approve preview buttons
    document.querySelectorAll('.preview-ad-btn').forEach(btn => {
        btn.addEventListener('click', async () => {
            const id = btn.getAttribute('data-ad-id');
            const body = document.getElementById('approvePreviewBody');

            // Set the advertisement ID to the approve and reject hidden input values
            approveInput.value = id;
            rejectInput.value = id;
                  
            body.innerHTML = `<div class="p-4 text-center text-muted">Laddar förhandsvisning…</div>`;
            try {
                const resp = await fetch(`/admin/annons/${id}/preview`);
                if (!resp.ok) {
                    const errorText = await resp.text();
                    body.innerHTML =
                        `<p class="p-4 text-danger">(${resp.status}) ${errorText}</p>`;
                } else {
                    body.innerHTML = await resp.text();

                    // Initialize carousel + thumbnails inside modal
                    setTimeout(() => {
                        initializeCarouselAndThumbnails(body);

                        const carouselEl = body.querySelector('#adCarousel');
                        if (carouselEl) {
                            new bootstrap.Carousel(carouselEl, {
                                ride: false,
                                interval: false
                            });
                        }
                    }, 50);
                }
            } catch (e) {
                body.innerHTML = `<p class="p-4 text-danger">Nätverksfel: ${e.message}</p>`;
            }
            // Show the modal
            new bootstrap.Modal(document.getElementById("approvePreviewModal")).show();
        });
    });        
});

//============== CREATE ADVERTISMENT LOGIC ==============

// ------- IMAGES --------

/**
 * This script handles:
 *  - Initializing the slots with either an empty array (Create) or existingImages (Edit)
 *  - AJAX upload of new images
 *  - Drag&drop reordering via Sortable.js
 *  - AJAX deletion of already-saved images
 *  - Keeping hidden inputs in sync for MVC model binding
 */

let selectedUrls = []; 
let deletedUrls = []; 

document.addEventListener('DOMContentLoaded', () => {   
    const slotsEl = document.getElementById('clickzone-slots');
    const fileInput = document.getElementById('imageFiles');
    const hiddenUrlsDiv = document.getElementById('hidden-image-urls');
    const hiddenOrderDiv = document.getElementById('hidden-image-order');
    const hiddenDeletedDiv = document.getElementById('hidden-deleted-image-urls');
    const form = document.getElementById('createAdForm') || document.getElementById('editAdForm');

    if (!slotsEl || !fileInput || !form) return;

    // If running in Edit mode, fill selectedUrls with existing images
    if (window.isEditMode) {
        const existing = window.existingImages || [];
        selectedUrls = existing.map(img => ({
            id: img.id,
            url: img.url,
            isNew: false
        }));
    }
    else { // If in Create mode, check localStorage for images
        const stored = localStorage.getItem('createAdImages');
        if (stored) {
            try {
                const arr = JSON.parse(stored);
                if (Array.isArray(arr)) {
                    // Add any URLs from localStorage that are not already in selectedUrls
                    selectedUrls = arr
                        .map(o => ({ url: o.url }))
                        .filter(o => typeof o.url === 'string' && o.url.trim())
                        .map(o => ({ id: null, url: o.url, isNew: true }));
                }
            } catch { console.warn('Invalid JSON in localStorage'); }
        }
    }

    // Clear localStorage on create form submit
    form.addEventListener('submit', () => {
        if (!window.isEditMode) localStorage.removeItem('createAdImages');
    });    

    // Initialize Sortable for sorting slots
    Sortable.create(slotsEl, {
        animation: 150,
        onEnd: (evt) => {
            // After drag, rebuild selectedUrls in new order
            const item = selectedUrls.splice(evt.oldIndex, 1)[0];
            selectedUrls.splice(evt.newIndex, 0, item);
            renderSlots();
            renderHiddenInputs();
            renderDeletedInputs();
            saveToStorage();
        }
    });

    // Handle click on placeholder slot to open file dialog
    slotsEl.addEventListener('click', e => {
        if (e.target.closest('.placeholder')) {
            fileInput.click();
        }
    });

    // When user selects a file via dialog
    fileInput.addEventListener('change', async () => {
        const file = fileInput.files[0];
        if (!file || !file.type.startsWith('image/')) return;

        // Upload via AJAX
        const fd = new FormData();
        fd.append('file', file);

        const resp = await fetch('/skapa-annons/upload-image', {
            method: 'POST',
            body: fd
        });
        if (!resp.ok) {
            alert('Fel vid filuppladdning');
            return;
        }
        const { url, id } = await resp.json();
        // Add to UI array
        selectedUrls.push({ id: id || null, url, isNew: true });
        renderSlots();
        renderHiddenInputs();
        renderDeletedInputs();
        saveToStorage();
        fileInput.value = '';  // reset
    });

    // Render slots: filled + one placeholder
    function renderSlots() {
        // Clear out any invalid URLs
        selectedUrls = selectedUrls.filter(item => typeof item.url === 'string' && item.url.trim());

        // Clear out any existing slot elements
        slotsEl.innerHTML = '';

        // Loop through each selected URL object
        selectedUrls.forEach((item, idx) => {          
            // Create the slot container
            const slot = document.createElement('div');

            // Save index and id in data attributes for easy access
            slot.dataset.idx = idx;
            if (item.id) slot.dataset.id = item.id;

            slot.className = 'cz-slot filled';
            slot.innerHTML = `<img src="${item.url}" class="cz-img" /><button type="button" class="cz-remove"><i class="bi bi-trash-fill"></i></button>`;

            // Attach click handler for the “remove” button
            slot.querySelector('.cz-remove').addEventListener('click', async () => {
                const i = Number(slot.dataset.idx);
                const entry = selectedUrls[i];
                
                if (window.isEditMode && !entry.isNew && entry.url) { // If in edit mode, we need to collect deleted URLs
                    deletedUrls.push(entry.url);
                }
                else if (!window.isEditMode && entry.url) {   // If in create mode, remove via AJAX   
                    // Grab the antiforgery token from the <meta> tag in Layout
                    const token = document.querySelector('meta[name="csrf-token"]').getAttribute('content');

                    const form = new FormData(); // Create a new FormData object
                    form.append('ImageUrl', entry.url); // Include the URL to delete
                    form.append('__RequestVerificationToken', token); // Add the CSRF token

                    // Prepare the payload for the POST request
                    const resp = await fetch(`/skapa-annons/delete-image`, {
                        method: 'POST',
                        body: form,
                        headers: {
                            'RequestVerificationToken': token,
                            'X-Requested-With': 'XMLHttpRequest'
                        }
                    });

                    if (!resp.ok) {
                        const err = await resp.json();
                        return alert('Kunde inte ta bort bilden: ' + (err.message || 'Okänt fel'));
                    }
                }                
                
                // Remove the image from the UI array and re-render
                selectedUrls.splice(i, 1);
                renderSlots();
                renderHiddenInputs();
                renderDeletedInputs();
                saveToStorage();
            });

            // Append the slot to the container
            slotsEl.append(slot);
        });

        // If there are fewer than 10 images, show the placeholder to add more
        if (selectedUrls.length < 10) {
            const placeholder = document.createElement('div');
            placeholder.className = 'cz-slot placeholder';
            placeholder.innerHTML = `<i class="bi bi-plus-lg cz-plus"></i>`;
            slotsEl.append(placeholder);
        }
    }

    // Sync hidden inputs for MVC
    function renderHiddenInputs() {
        hiddenUrlsDiv.innerHTML = '';
        hiddenOrderDiv.innerHTML = '';

        selectedUrls.forEach((item, idx) => {
            // URL input
            const urlInput = document.createElement('input');
            urlInput.type = 'hidden';
            urlInput.name = 'ImageUrls';
            urlInput.value = item.url;
            hiddenUrlsDiv.append(urlInput);

            // Order input
            const orderInput = document.createElement('input');
            orderInput.type = 'hidden';
            orderInput.name = 'ImageOrder';
            orderInput.value = `${idx}:${idx}`;
            hiddenOrderDiv.append(orderInput);
        });
    }

    // Sync hidden inputs for deleted images
    function renderDeletedInputs() {
        if (!window.isEditMode) return;
        hiddenDeletedDiv.innerHTML = '';
        deletedUrls.forEach(url => {
            const inp = document.createElement('input');
            inp.type = 'hidden';
            inp.name = 'DeletedImageUrls';
            inp.value = url;
            hiddenDeletedDiv.append(inp);
        });
    }
    function saveToStorage() {
        if (!window.isEditMode) {
            localStorage.setItem('createAdImages',
                JSON.stringify(selectedUrls.map(o => ({ url: o.url })))
            );
        }
    }

    // Initial render
    renderSlots();
    renderHiddenInputs();
    renderDeletedInputs();
});


// ------- MODAL PREVIEW --------
/**
 * Click handler for "Preview" button.
 * Sends same payload to /skapa-annons/preview via POST,
 * injects the returned partial into the modal, and shows it.
 */
document.addEventListener('DOMContentLoaded', () => {
    const previewBtn = document.getElementById("previewButton");
    if (previewBtn) {
        previewBtn.addEventListener("click", async () => {
            const modalBody = document.getElementById("previewModalBody");
            modalBody.innerHTML = `<div class="p-4 text-center text-muted">Laddar förhandsvisning…</div>`;

            const payload = getFormData(document.getElementById("createAdForm"));
            
            try {
                const resp = await fetch('/skapa-annons/preview', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(payload)
                });

                if (!resp.ok) {
                    const errorText = await resp.text();
                    modalBody.innerHTML =
                        `<p class="p-4 text-danger">(${resp.status}) ${errorText}</p>`;
                } else {
                    modalBody.innerHTML = await resp.text();

                    // Initialize carousel + thumbnails inside modal
                    setTimeout(() => {
                        initializeCarouselAndThumbnails(modalBody);

                        const carouselEl = modalBody.querySelector('#adCarousel');
                        if (carouselEl) {
                            new bootstrap.Carousel(carouselEl, {
                                ride: false,
                                interval: false
                            });
                        }
                    }, 50);
                }
            } catch (err) {
                modalBody.innerHTML = `<p class="p-4 text-danger">Nätverksfel: ${err.message}</p>`;
            }

            // Show the modal
            new bootstrap.Modal(document.getElementById("previewModal")).show();
        });
    }
});

/**
 * Gather form values into a JSON-friendly object.
 * Uses HTML5 APIs (.valueAsNumber, .valueAsDate) where appropriate.
 */
function getFormData(form) {
    // Helper to grab an input/select/textarea by its name attribute
    const get = name => form.querySelector(`[name="${name}"]`);

    // Collect the blob-URLs for the thumbnails
    const preview = document.getElementById('clickzone-slots'); 
    const images = preview
        ? Array.from(preview.querySelectorAll('img.cz-img')).map(img => ({ Url: img.src }))
        : [];

    // Read the ad type from the hidden input, parse it to integer, or null if empty
    const raw = get('AdType').value;
    const parsed = parseInt(raw, 10);
    const adType = (!raw || isNaN(parsed)) ? null : parsed;

    return {
        // If the category select is blank, send 0; otherwise parse to integer
        CategoryId: parseInt(get('CategoryId').value || '0', 10),

        // Blob URLs
        Images: images,
                        
        // Read optional video URL
        VideoURL: get('VideoURL').value || null,

        // Basic text fields: title, description (or null)
        Title: get('Title').value || null,
        Description: get('Description').value || null,

        // Parse Condition to integer or null
        Condition: get('Condition').value ? parseInt(get('Condition').value, 10) : null,
        AdType: adType,

        // Numbers: use .valueAsNumber, but guard against NaN
        StartingPrice: isNaN(get('StartingPrice').valueAsNumber) ? null : get('StartingPrice').valueAsNumber,
        BuyNowPrice: isNaN(get('BuyNowPrice').valueAsNumber) ? null : get('BuyNowPrice').valueAsNumber,
        MinimumEndPrice: isNaN(get('MinimumEndPrice').valueAsNumber) ? null : get('MinimumEndPrice').valueAsNumber,

        // Boolean and text fields for pickup/shipping
        AvailableForPickup: get('AvailableForPickup').checked,
        PickupLocation: get('PickupLocation').value || null,
        ShippingMethod: get('ShippingMethod').value || null,

        // Collect all checked payment method checkboxes
        PaymentMethods: Array.from(
            form.querySelectorAll('input[name="PaymentMethods"]:checked')
        ).map(cb => cb.value),

        // Parse auction end date, or null if not set
        AuctionEndDate: get('AuctionEndDate').value ? new Date(get('AuctionEndDate').value).toISOString() : null,

        // Company seller checkbox
        IsCompanySeller: get('IsCompanySeller').checked,
                
    };
};
/**
 * Adds a new image URL input field to the form and immediately triggers a preview update.
 */
window.addImageField = function () {
    const container = document.getElementById("imageUrlList");

    // Determine the next index based on the current number of image inputs
    const currentInputs = container.querySelectorAll('input[name^="Images"][name$=".Url"]');
    const nextIndex = currentInputs.length;

    const div = document.createElement("div");
    div.className = "input-group mb-2";
    div.innerHTML = `
    <input name="Images[${nextIndex}].Url"
           class="form-control"
           placeholder="https://example.com/another.jpg" />
    <button type="button" class="btn btn-outline-danger" onclick="removeImage(this)">
      Ta bort
    </button>`;
    container.appendChild(div);
};
/**
 * Removes the specified image URL input field, re-indexes the remaining fields,
 * and triggers a preview update.
 */
window.removeImage = function (button) {
    // Remove the entire input group containing this button
    button.closest(".input-group").remove();

    // Recalculate indexes on all remaining image inputs for proper model binding
    const container = document.getElementById("imageUrlList");
    const inputs = container.querySelectorAll('input[name^="Images"][name$=".Url"]');
    inputs.forEach((input, idx) => {
        input.setAttribute("name", `Images[${idx}].Url`);
    });
};

// ============= DATE & COUNTDOWN LOGIC =============

(() => {
    // ====== Utility Functions ======

    /**
     * Formats the remaining time into a readable string.
     * @param {number} ms - Milliseconds remaining.
     * @returns {string} e.g. "2h 15m", "45m 30s", "10s", or "Avslutad" if time is up.
     */
    function formatTimeRemaining(ms) {
        if (ms <= 0) return 'Avslutad';

        const totalSeconds = Math.floor(ms / 1000);
        const days = Math.floor(totalSeconds / 86400);
        const hours = Math.floor((totalSeconds % 86400) / 3600);
        const minutes = Math.floor((totalSeconds % 3600) / 60);
        const seconds = totalSeconds % 60;

        if (days > 0) {
            if (hours > 0) {
                return `${days} dag${days > 1 ? 'ar' : ''} ${hours} h`;
            } else {
                return `${days} dag${days > 1 ? 'ar' : ''}`;
            }
        }
        if (hours > 0) return `${hours} h ${minutes} min`;
        if (minutes > 0) return `${minutes} m ${seconds} sek`;
        return `${seconds} sek`;
    }

    /**
     * Formats the end date of an advertisement for display in various contexts.
     * @param {string} dateString - ISO date string.
     * @param {string} context - 'card', 'detail', 'modal'
     * @returns {string} Formatted date string.
     */
    function formatEndDate(dateString, context) {
        const endDate = new Date(dateString);
        const now = new Date();

        const tomorrow = new Date();
        tomorrow.setDate(now.getDate() + 1);
        const isTomorrow = endDate.toDateString() === tomorrow.toDateString();

        // Format time as HH:mm
        const timeString = endDate.toLocaleTimeString('sv-SE', {
            hour: '2-digit',
            minute: '2-digit',
        });

        // Determine how to format the date based on context
        if (context === 'card') {
            const timeDiffMs = endDate - now;
            const hoursLeft = timeDiffMs / (1000 * 60 * 60);

            if (hoursLeft < 24) {
                // Same day or tomorrow but less than 24h -> show only time
                return timeString;
            }

            if (isTomorrow) {
                return `Imorgon ${timeString}`; // If it's tomorrow, show "Imorgon [time]"
            }

            // Else, show full date + time
            const dateParts = endDate.toLocaleDateString('sv-SE', {
                day: 'numeric',
                month: 'short',
            }).split(' '); // split into day and month

            const day = dateParts[0];
            const month = dateParts[1].substring(0, 3).toLowerCase(); // ensure lowercase 3-letter month

            return `${day} ${month} ${timeString}`;
        }

        // For detail view or modal (always full date + time)
        const dateParts = endDate.toLocaleDateString('sv-SE', {
            day: 'numeric',
            month: 'short',
        }).split(' ');

        const day = dateParts[0];
        const month = dateParts[1].substring(0, 3).toLowerCase(); // ensure lowercase 3-letter month

        return `${day} ${month} ${timeString}`;
    }

    // ====== Countdown Update Logic ======

    /**
     * Updates all countdown elements (.countdown) on the page,
     * adjusting displayed text and styles according to context and time left.
     */
    function updateCountdownDisplays() {
        const now = Date.now();
        const countdownElements = Array.from(document.querySelectorAll('.countdown'));

        countdownElements.forEach(el => {
            const endTime = new Date(el.dataset.end).getTime(); // Get auction end time
            const context = el.dataset.context; // 'card', 'detail', 'modal'
            const msLeft = endTime - now; // Calculate milliseconds left for the auction
            const twelveHoursMs = 12 * 60 * 60 * 1000; // 12 hours in milliseconds, for checking if countdown should be styled as warning
            const eightDaysMs = 8 * 24 * 60 * 60 * 1000; // 8 days in milliseconds, for detail view countdown visibility
            const isEnded = el.dataset.isEnded === 'true'; // Check if auction has ended

            // Ensure the element has the necessary spans for date, separator, and timer
            let dateSpan = el.querySelector('.countdown-date');
            let separatorSpan = el.querySelector('.countdown-separator'); // Middle dot, between between date and countdown, for detail view
            let timerSpan = el.querySelector('.countdown-timer');

            // Create spans if they don't exist

            if (!dateSpan) {
                dateSpan = document.createElement('span');
                dateSpan.classList.add('countdown-date');
                el.appendChild(dateSpan);
            }

            if (context !== 'card') { // Only add middle dot separator for detail and modal contexts
                if (!separatorSpan) {
                    separatorSpan = document.createElement('span');
                    separatorSpan.classList.add('countdown-separator');
                    el.appendChild(separatorSpan);
                }
            }

            if (!timerSpan) {
                timerSpan = document.createElement('span');
                timerSpan.classList.add('countdown-timer');
                el.appendChild(timerSpan);
            }

            // Format the end date text for display
            const dateText = formatEndDate(el.dataset.end, context);

            // Determine if countdown should be shown for the contexts detail and card
            const showCountdown =
                (context === 'detail' && msLeft <= eightDaysMs) || // less than 8 days left in detail view
                (context === 'card' && msLeft <= 24 * 60 * 60 * 1000);  // less than 24 hours left on card

            if (context === 'modal') {
                // MODAL: only countdown should be shown
                timerSpan.textContent = formatTimeRemaining(msLeft) || ''; 
                dateSpan.textContent = '';
                if (separatorSpan) separatorSpan.textContent = '';
                timerSpan.classList.toggle('countdown-warning', msLeft > 0 && msLeft <= twelveHoursMs); // highlight if less than 12 hours left
            }
            else if (context === 'detail') {
                // DETAIL VIEW: show dateText, and countdown if applicable
                if (isEnded) {
                    // If auction has ended, show "Avslutad" text
                    dateSpan.textContent = `Avslutad ${dateText}`; 
                    if (separatorSpan) separatorSpan.textContent = '';
                    timerSpan.textContent = '';
                    timerSpan.classList.remove('countdown-warning');
                }
                else {
                    // If auction is still ongoing, show end date, and countdown if applicable

                    dateSpan.textContent = `Slutar ${dateText}`; 

                    if (showCountdown) {
                        // Only show middle dot separator if countdown is visible
                        if (separatorSpan) separatorSpan.textContent = ' • ';
                        timerSpan.textContent = formatTimeRemaining(msLeft);
                        timerSpan.classList.toggle('countdown-warning', msLeft > 0 && msLeft <= twelveHoursMs); // highlight if less than 12 hours left
                    } else {
                        // If countdown is not shown, clear the separator and timer
                        if (separatorSpan) separatorSpan.textContent = '';
                        timerSpan.textContent = '';
                        timerSpan.classList.remove('countdown-warning');
                    }
                }
            }
            else if (context === 'card') {
                // CARD: show dateText, and countdown if applicable, no middle dot separator

                dateSpan.textContent = dateText;

                if (showCountdown) {
                    // If countdown is applicable, show it
                    if (separatorSpan) separatorSpan.textContent = ''; // Clear separator for card context
                    timerSpan.textContent = formatTimeRemaining(msLeft);
                    timerSpan.classList.toggle('countdown-warning', msLeft > 0 && msLeft <= twelveHoursMs); // highlight if less than 12 hours left
                }
                else {
                    // If countdown is not applicable, clear the timer
                    if (separatorSpan) separatorSpan.textContent = '';
                    timerSpan.textContent = '';
                    timerSpan.classList.remove('countdown-warning');
                }
            }
            else {
                // Unknown context, throw an error
                throw new Error(`Unknown context '${context}' in updateCountdownDisplays`);
            }
        });
    }

    // Initial update and refresh every second
    updateCountdownDisplays();
    setInterval(updateCountdownDisplays, 1000);

    // Update countdown when modal for bidding is shown
    const bidModal = document.getElementById('bidModal');
    if (bidModal) {
        bidModal.addEventListener('shown.bs.modal', () => {
            setTimeout(updateCountdownDisplays, 50); // tiny delay to ensure DOM ready
        });
    }
})();

// ====== Favourite-Toggle Logic ======
/**
    * Attaches a click listener to the document body.
    * When a .favourite-toggle button is clicked, sends a POST request
    * to toggle the favourite state and updates the button class.
    */
async function initFavouriteToggles() {
    document.body.addEventListener('click', async (e) => {
        const btn = e.target.closest('.favourite-toggle');
        if (!btn) return; // ignore clicks outside the button

        e.preventDefault();
        e.stopPropagation();

        const adId = btn.dataset.adId;
        const token = document.querySelector('meta[name="csrf-token"]').getAttribute('content');

        try {
            const response = await fetch('/Advertisement/ToggleFavourite', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': token
                },
                body: JSON.stringify({ advertisementId: adId })
            });

            if (!response.ok) {
                throw new Error(`Server returned ${response.status}`);
            }
            const data = await response.json();

            // Toggle the 'favourited' class based on server response
            btn.classList.toggle('favourited', Boolean(data.favourited));
        } catch (err) {
            console.error('Error toggling favourite:', err);
        }
    });
};

// ====== Initialization on DOM Ready ======

document.addEventListener('DOMContentLoaded', () => {
    initFavouriteToggles();
});

//============== EDIT ADVERTISMENT LOGIC ==============

// ------- MODAL PREVIEW --------

document.addEventListener('DOMContentLoaded', () => {
    const previewBtn = document.getElementById("editPreviewButton");
    if (previewBtn) {
        previewBtn.addEventListener("click", async () => {
            const modalBody = document.getElementById("previewModalBody");
            modalBody.innerHTML = `<div class="p-4 text-center text-muted">Laddar förhandsvisning…</div>`;

            const payload = getFormData(document.getElementById("editAdForm"));

            try {
                const resp = await fetch('/redigera-annons/preview', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(payload)
                });

                if (!resp.ok) {
                    const errorText = await resp.text();
                    modalBody.innerHTML =
                        `<p class="p-4 text-danger">(${resp.status}) ${errorText}</p>`;
                } else {
                    modalBody.innerHTML = await resp.text();

                    // Initialize carousel + thumbnails inside modal
                    setTimeout(() => {
                        initializeCarouselAndThumbnails(modalBody);

                        const carouselEl = modalBody.querySelector('#adCarousel');
                        if (carouselEl) {
                            new bootstrap.Carousel(carouselEl, {
                                ride: false,
                                interval: false
                            });
                        }
                    }, 50);
                }
            } catch (err) {
                modalBody.innerHTML = `<p class="p-4 text-danger">Nätverksfel: ${err.message}</p>`;
            }

            // Show the modal
            new bootstrap.Modal(document.getElementById("previewModal")).show();
        });
    }
});


/*---------------------------------------------------------------------------------------------*/
                                          /*Chat*/
/*---------------------------------------------------------------------------------------------*/

// ================== CHAT DOT SIGNALR ==================

if (typeof signalR !== "undefined") {
    // Initialize SignalR connection for chat notifications
    const chatHubConnection  = new signalR.HubConnectionBuilder()
        .withUrl("/chathub")
        .build();

    chatHubConnection .start()
        .then(() => {
            console.log("Chat navbar SignalR connected");
            chatHubConnection .invoke("JoinUserGroup");
        })
        .catch(err => console.error("Failed to connect to chat nav SignalR:", err));

    // Update chat dot visibility based on unread messages. 
    // One red dot is shown in the navbar and one for each conversation in the chat list.
    chatHubConnection.on("ReceiveUnreadMessage", (chatId) => {
        // Get the chat icon dot elements for desktop and mobile
        const dotDesktop = document.getElementById("chat-dot-desktop");
        const dotMobile = document.getElementById("chat-dot-mobile");

        // Show the chat icon dot indicators
        if (dotDesktop) dotDesktop.style.display = "inline-block";
        if (dotMobile) dotMobile.style.display = "inline-block";

        // Get the chat item (conversation) in the chat list
        const chatItem = document.querySelector(`[data-chat-id='${chatId}']`);

        if (chatItem && !chatItem.querySelector('.chat-unread-dot')) {
            // If the chat item exists and doesn't already have a dot, create and prepend it
            const dot = document.createElement('span');
            dot.classList.add('chat-unread-dot');
            chatItem.prepend(dot);
        }
    });

    // Hide the chat icon dot when all messages are read
    chatHubConnection .on("AllMessagesRead", () => {
        const dotDesktop = document.getElementById("chat-dot-desktop");
        const dotMobile = document.getElementById("chat-dot-mobile");

        if (dotDesktop) dotDesktop.style.display = "none";
        if (dotMobile) dotMobile.style.display = "none";
    });

    // Hide the unread dot for a specific chat conversation when all messages in it have been read
    chatHubConnection .on("ChatConversationRead", (chatId) => {
        const chatItem = document.querySelector(`[data-chat-id='${chatId}']`);
        const dot = chatItem?.querySelector('.chat-unread-dot');
        if (dot) dot.remove();
    });
}

document.addEventListener("DOMContentLoaded", () => {

    // Check for unread messages and show the chat icon dot if necessary
    const chatDotDesktop = document.getElementById("chat-dot-desktop");
    const chatDotMobile = document.getElementById("chat-dot-mobile");

    fetch('/Chat/HasUnreadMessages')
        .then(response => response.json())
        .then(data => {
            const showDot = data.hasUnread;
            if (chatDotDesktop) chatDotDesktop.style.display = showDot ? "inline-block" : "none";
            if (chatDotMobile) chatDotMobile.style.display = showDot ? "inline-block" : "none";
        })
        .catch(error => console.error("Could not check for unread messages:", error));

    // Get the "Kontakta säljaren" button and the chat modal
    const contactBtn = document.getElementById("contactSellerBtn");
    const chatModal = document.getElementById("chatModal");

    if (!contactBtn || !chatModal) {
        return;
    }

    // When "Kontakta säljaren" button is clicked
    contactBtn.addEventListener("click", async () => {
        const adId = contactBtn.dataset.advertisementId;

        // Check if a chat already exists for this advertisement
        const res = await fetch(`/Chat/CheckExistingChat?advertisementId=${adId}`);
        const data = await res.json();

        if (data.exists) {
            // Redirect to existing chat
            window.location.href = `/Chat/ViewChat?chatId=${data.chatId}`;
        } else {
            // Open modal to write the first message
            chatModal.style.display = "block";
        }
    });

    const form = document.getElementById("firstMessageForm");

    // Handle first message submission inside the modal
    form.addEventListener("submit", async (e) => {
        e.preventDefault();

        const adId = document.getElementById("hiddenAdId").value;

        const content = document.getElementById("messageInput").value;

        const tokenInput = document.querySelector("input[name='__RequestVerificationToken']");

        if (!tokenInput) {
            console.error("CSRF token not found");
            return;
        }

        const token = tokenInput.value;

        // Send the first message via POST request
        const res = await fetch("/Chat/SendFirstMessage", {
            method: "POST",
            headers: {
                "Content-Type": "application/x-www-form-urlencoded",
                "RequestVerificationToken": token // Include CSRF token for security
            },
            body: `advertisementId=${encodeURIComponent(adId)}&content=${encodeURIComponent(content)}`
        });

        if (res.ok) {
            // Inform the user and close the modal
            alert('Ditt meddelande har skickats!');
            chatModal.style.display = "none";
        } else {
            alert("Det gick inte att skicka meddelandet.");
        }
    });

    // Allow modal to be closed from close button (calls window.closeModal from HTML)
    window.closeModal = () => {
        chatModal.style.display = "none";
    };
});

document.addEventListener("DOMContentLoaded", function () {
    const container = document.getElementById("chat-messages");

    // Exit early if no chat container is found (i.e., not in a chat view)
    if (!container) return;

    let newMessageCount = 0;

    // Scroll to the bottom of the chat on page load
    container.scrollTop = container.scrollHeight;

    // Attach scroll listener to hide the "Nytt meddelande" banner when user scrolls to bottom of the chat
    container.addEventListener("scroll", () => {
        const messages = container.querySelectorAll(".chat-bubble");
        const lastMessage = messages[messages.length - 1];

        if (!lastMessage) return;

        const rect = lastMessage.getBoundingClientRect(); // Get the bounding rectangle of the last message
        const containerRect = container.getBoundingClientRect(); // Get the bounding rectangle of the chat container

        // Check if the bottom of the last message is within the visible area
        const isLastMessageVisible = rect.bottom <= containerRect.bottom;

        if (isLastMessageVisible) {
            removeInlineNewMessageBanner();
        }
    });


    // Create an IntersectionObserver to detect when unread messages enter the viewport
    // This will be used to mark messages as read when they are viewed
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) { // If the message is fully visible in the viewport
                const messageEl = entry.target;
                const messageId = messageEl.getAttribute("data-message-id");
                const chatId = messageEl.getAttribute("data-chat-id");

                // Only mark as read if the message is still marked as unread
                if (!messageEl.classList.contains("unread")) return;

                // Send a POST request to mark the message as read
                fetch("/Chat/MarkMessageAsRead", {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                    },
                    body: JSON.stringify({
                        MessageId: parseInt(messageId),
                        ChatId: parseInt(chatId)
                    })
                })
                .then(response => {
                    if (response.ok) {
                        messageEl.classList.remove("unread");  // Remove unread styling
                        observer.unobserve(messageEl);         // Stop observing this message
                    }
                })
                .catch(error => console.error("Failed to mark message as read:", error));
            }
        });
    }, {
        threshold: 1.0 // Trigger only when 100% of the message is visible
    });

    // Observe all currently unread chat bubbles
    document.querySelectorAll(".chat-bubble.unread").forEach(el => {
        observer.observe(el);
    });

    // Get the current user's ID from the DOM
    const metadataDiv = document.getElementById("chat-metadata");
    const currentUserId = metadataDiv?.getAttribute("data-current-user-id");

    if(!currentUserId) {
        console.warn("Current user ID not found in chat-metadata div");
    }

    // Get the chat ID from a hidden input field
    const chatId = parseInt(document.getElementById("chat-id").value);

    // Abort if chat ID is missing
    if (!chatId) {
        console.error("Missing chat ID");
        return;
    }

    // Retrieve CSRF token required for secure AJAX POSTs (ASP.NET Core anti-forgery)
    const csrfToken = document.querySelector('meta[name="csrf-token"]').getAttribute('content');

    if (!csrfToken) {
        console.error("Missing CSRF token");
        return;
    }

    // Initialize SignalR connection to the server chat hub
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/chathub")
        .build();

    // Start the SignalR connection and join the chat group for this chat ID
    connection.start().then(() => {
        connection.invoke("JoinChatGroup", chatId)
            .catch(err => console.error("JoinChatGroup error:", err));
    }).catch(err => console.error(err.toString()));

    // When receiving a message via SignalR
    // Create chat bubble with the message and append it to the rest of the messages
    // These div's that are appended match exactly the ones in the partial view, ChatMessageBubble
    connection.on("ReceiveMessage", function (message) {

        const isOwn = message.senderId === currentUserId;
        const alignment = isOwn ? "justify-content-end" : "justify-content-start";
        const bubbleColor = isOwn ? "#DCF8C6" : "#E4E6EB";
        const unreadClass = isOwn || message.isRead ? "" : "unread";

        const chatContainer = document.getElementById("chat-messages");
        console.log("chatContainer:", chatContainer);

        // Create outer alignment wrapper
        const outerDiv = document.createElement("div");
        outerDiv.className = `d-flex ${alignment} mb-2`;

        // Create inner flex-column container
        const columnDiv = document.createElement("div");
        columnDiv.className = "d-flex flex-column align-items-start";
        columnDiv.style.width = "70%";

        // Add sender name
        const senderNameDiv = document.createElement("div");
        senderNameDiv.className = "fw-bold text-muted mb-1 text-start";
        senderNameDiv.style.width = "100%";
        senderNameDiv.textContent = message.senderName;

        // Add message bubble
        const bubbleDiv = document.createElement("div");
        bubbleDiv.className = `chat-bubble p-2 rounded ${unreadClass}`;
        bubbleDiv.style.width = "100%";
        bubbleDiv.style.backgroundColor = bubbleColor;
        bubbleDiv.setAttribute("data-message-id", message.id);
        bubbleDiv.setAttribute("data-chat-id", message.chatId);
        bubbleDiv.setAttribute("data-sender-id", message.senderId);
        bubbleDiv.setAttribute("data-sent-at", message.sentAt);
        bubbleDiv.setAttribute("data-is-own-message", isOwn.toString().toLowerCase());
        bubbleDiv.setAttribute("data-is-read", message.isRead.toString().toLowerCase());

        // Add div for message content
        const contentDiv = document.createElement("div");
        contentDiv.className = "message-content text-start";
        contentDiv.textContent = message.content;

        bubbleDiv.appendChild(contentDiv);

        // Add meta info
        const metaDiv = document.createElement("div");
        metaDiv.className = "message-meta text-muted small mt-1 text-start";
        metaDiv.style.width = "100%";
        metaDiv.innerHTML = formatMessageTimeWithReadStatus(message.sentAt, isOwn, message.isRead);

        // Assemble the column
        columnDiv.appendChild(senderNameDiv);
        columnDiv.appendChild(bubbleDiv);
        columnDiv.appendChild(metaDiv);

        // Assemble the row
        outerDiv.appendChild(columnDiv);

        console.log("outerDiv:", outerDiv);
        chatContainer.appendChild(outerDiv);
        

        observer.observe(bubbleDiv); // Start observing the new message bubble for read status

        const isNearBottom = isSecondToLastMessageAlmostVisible(chatContainer);

        // If user is near the bottom of the chat, or new message was sent by the user, scroll to bottom and remove the "new message" banner (if it exists)
        if (isNearBottom || isOwn) {
            chatContainer.scrollTop = chatContainer.scrollHeight;
            removeInlineNewMessageBanner();
        } else {
            showInlineNewMessageBanner();
        }
    });

    // Display read receipts for specific messages when notified by the server
    connection.on("MessageRead", messageId => {
        const messageElement = document.querySelector(`[data-message-id="${messageId}"]`);
        if (!messageElement) return;

        const isOwnMessage = messageElement.dataset.isOwnMessage === "true";
        const isRead = true;

        const sentAt = messageElement.dataset.sentAt;
        const metaElement = messageElement.parentElement.querySelector(".message-meta");

        if (metaElement) {
            metaElement.textContent = formatMessageTimeWithReadStatus(sentAt, isOwnMessage, isRead);
        }

        messageElement.dataset.isRead = "true";
    });



    // Handle chat message form submission via AJAX
    document.getElementById("sendMessageForm").addEventListener("submit", function (event) {
        event.preventDefault(); // Prevent full page reload

        const messageInput = document.getElementById("message-input");
        const content = messageInput.value.trim();

        if (!content) return; // Skip empty messages

        // Send the message to the server
        fetch('/Chat/SendMessage', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': csrfToken
            },
            body: JSON.stringify({
                chatId: chatId,
                content: content
            })
        }).then(response => {
            if (response.ok) {
                messageInput.value = ''; // Clear input after successful send
            } else {
                alert("Något gick fel vid skickandet av meddelandet.");
                console.error("Failed to send message.");
            }
        });
    });
});

function showInlineNewMessageBanner() {
    let banner = document.getElementById("newMessageBanner");

    if (!banner) {
        // Create the banner for the first time
        banner = document.createElement("div");
        banner.id = "newMessageBanner";
        banner.classList.add("new-message-banner");

        // Add click handler
        banner.onclick = () => {
            const container = document.getElementById("chat-messages");
            container.scrollTop = container.scrollHeight;
            removeInlineNewMessageBanner();
            newMessageCount = 0; // Reset counter
        };

        // Add to the DOM
        document.getElementById("chat-banner-anchor").appendChild(banner);
    }

    // Update the text based on number of new messages
    newMessageCount++;
    banner.innerText = newMessageCount === 1
        ? "Nytt meddelande – klicka för att scrolla ner"
        : `${newMessageCount} nya meddelanden`;
}

function removeInlineNewMessageBanner() {
    const banner = document.getElementById("newMessageBanner");
    if (banner) banner.remove();
    newMessageCount = 0;
}

// Function to check if the second to last message is almost visible in the chat window
function isSecondToLastMessageAlmostVisible(container) {
    const bubbles = container.querySelectorAll(".chat-bubble");
    if (bubbles.length < 2) return false; // not enough messages to compare

    const secondToLast = bubbles[bubbles.length - 2];
    const rect = secondToLast.getBoundingClientRect();
    const containerRect = container.getBoundingClientRect();

    // How much of the second-to-last bubble is visible
    const visibleHeight = Math.min(rect.bottom, containerRect.bottom) - Math.max(rect.top, containerRect.top);

    // If more than 75% of the bubble is visible, return true
    return visibleHeight / rect.height >= 0.75;
}

// Format the displayed timestamp of a message along with its read status
function formatMessageTimeWithReadStatus(isoDateTime, isOwnMessage, isRead) {
    const date = new Date(isoDateTime);

    const now = new Date();
    const isToday = date.toDateString() === now.toDateString();

    const yesterday = new Date(now);
    yesterday.setDate(now.getDate() - 1);
    const isYesterday = date.toDateString() === yesterday.toDateString();


    // Format the time in 24-hour format with leading zeros
    const time = date.toLocaleTimeString("sv-SE", {
        hour: "2-digit",
        minute: "2-digit"
    });

    // Determine the date part based on whether it's today, yesterday, or another date
    let datePart;
    if (isToday) {
        datePart = "Idag";
    } else if (isYesterday) {
        datePart = "Igår";
    } else {
        datePart = date.toLocaleDateString("sv-SE", {
            day: "numeric",
            month: "short",
            year: "numeric"
        });
    }

    // Add read receipt only if it's the user's own message and it's marked as read
    const readText = isOwnMessage && isRead ? " ✅ Öppnat" : "";

    return `${datePart} ${time}${readText}`;
}


document.addEventListener('DOMContentLoaded', function () {

    const searchForm = document.getElementById('unified-search-form');
    const searchInput = document.getElementById('unified-search-input');
    const categoryDropdown = document.getElementById('unified-category-dropdown');
    let debounceTimer;

    function submitSearchForm() {
        if (searchForm) {
            searchForm.submit();
        }
    }

    if (searchForm) {
        // Submit form when category is changed
        if (categoryDropdown) {
            categoryDropdown.addEventListener('change', submitSearchForm);
        }

        // Submit form after user stops typing in search box
        if (searchInput) {
            searchInput.addEventListener('input', function (e) {
                clearTimeout(debounceTimer);
                debounceTimer = setTimeout(submitSearchForm, 500); // 500ms delay
            });
        }
    }
});

// ----------------- General ----------------------
if (window.jQuery) {
    $(function () {
        // Get the CSRF token from the meta tag
        const csrfToken = $('meta[name="csrf-token"]').attr('content');

        // Automatically add the CSRF token to all jQuery AJAX requests
        $.ajaxSetup({
            headers: {
                'RequestVerificationToken': csrfToken
            }
        });
    });
}
//============== SHARE FUNCTIONS ==============

document.addEventListener("DOMContentLoaded", () => {
    // Fetch the share menu elements after the DOM is fully loaded
    const shareMenu = document.getElementById("shareMenu");
    const overlay = document.getElementById("shareOverlay");
    const openBtn = document.getElementById("openShareMenuBtn");
    const closeBtn = document.getElementById("closeShareMenuBtn");
    const fullUrl = window.location.href; // Use the current page URL as the full URL of the ad

    // Toggle show & overlay
    function openMenu() {
        overlay.classList.add("visible");
        shareMenu.classList.add("show");
    }
    function closeMenu() {
        if (!shareMenu.classList.contains("show")) return;
        shareMenu.classList.add("fade-out");
        overlay.classList.remove("visible");
        setTimeout(() => shareMenu.classList.remove("show", "fade-out"), 200);
    }
    window.closeMenu = closeMenu;

    if (openBtn) {
        openBtn.addEventListener("click", openMenu);
    }
    if (closeBtn) {
        closeBtn.addEventListener("click", closeMenu);
    }
    if (overlay) {
        overlay.addEventListener("click", closeMenu);
    }
    // Close the share menu when pressing Escape key
    document.addEventListener("keydown", e => {
        if (e.key === "Escape") closeMenu();
    });
    
    // Share the ad via different platforms
    window.shareVia = platform => {
        const encodedUrl = encodeURIComponent(fullUrl);
        const isMobile = /android|iphone|ipad|ipod/i.test(navigator.userAgent);
        let shareUrl = "";

        switch (platform) {
            case "facebook": shareUrl = `https://m.facebook.com/sharer/sharer.php?u=${encodedUrl}`; break;
            case "x": shareUrl = `https://twitter.com/intent/tweet?url=${encodedUrl}`; break;
            case "whatsapp": shareUrl = `https://wa.me/?text=${encodedUrl}`; break;
            case "telegram": {
                copyLink(false);

                if (isMobile) {
                    window.open(`https://t.me/share/url?url=${encodedUrl}`, "_blank");
                }
                else {
                    showToast("Länken har kopierats till urklipp! Öppnar Telegram‑webben…", "Info", 3000, "info");
                    setTimeout(() => {                        
                        window.open(`https://t.me/share/url?url=${encodedUrl}`, "_blank");
                    }, 2000);
                }
                return;
            }
            case "email": shareUrl = `mailto:?subject=Kolla in denna annons&body=${encodedUrl}`; break;
            case "sms": shareUrl = `sms:?body=${encodedUrl}`; break;
            case "messenger": {
                copyLink(false);                

                if (isMobile) {
                    window.location.href = `fb-messenger://share?link=${encodedUrl}`;
                    setTimeout(() => {
                        window.open("https://www.facebook.com/sharer/sharer.php?u=" + encodedUrl, "_blank");
                    }, 1500);
                } else {
                    showToast("Direktdelning ej tillgängligt. Länken kopierad – öppnar Facebook…", "Info", 4000, "info");
                    setTimeout(() => {
                        window.open("https://www.facebook.com/", "_blank");
                    }, 3000);
                }
                return;
            }
            case "snapchat": {
                copyLink(false);                

                if (isMobile) {
                    window.location.href = `snapchat://add?link=${encodedUrl}`;
                    setTimeout(() => {
                        window.open("https://www.snapchat.com/add/", "_blank");
                    }, 1500);
                } else {
                    showToast("Direktdelning ej tillgängligt. Länken kopierad – öppnar Snapchat…", "Info", 4000, "info");
                    setTimeout(() => {
                        window.open("https://www.snapchat.com/add/", "_blank");
                    }, 3000);
                }
                return;
            }
            case "instagram": {
                copyLink(false);

                if (isMobile) {
                    window.location.href = `instagram://share?text=${encodedUrl}`;
                    setTimeout(() => {
                        window.open("https://www.instagram.com/", "_blank");
                    }, 1500);
                } else {
                    showToast("Direktdelning ej tillgängligt. Länken kopierad – öppnar Instagram…", "Info", 4000, "info");
                    setTimeout(() => {
                        window.open("https://www.instagram.com/", "_blank");
                    }, 3000);
                }
                return;
            }
            default: showToast("Delningsalternativet stöds ej i din webbläsare.", "Fel", 4000, "danger"); return;
        }
        window.open(shareUrl, '_blank');
    }

    // Copy the ad link to clipboard
    window.copyLink = (showToastMessage = true) => {
        navigator.clipboard.writeText(fullUrl)
            .then(() => {
                if (showToastMessage)
                    showToast("Länken har kopierats till urklipp!", "Klart", 2000, "success");
            })
            .catch(() => {
                showToast("Kunde inte kopiera länken. Manuell kopiering rekommenderas.", "Fel", 4000, "danger");
            });
    };
});

//============== DETAILED VIEW ==============

// This code initializes the carousel and thumbnail functionality on the advertisement details page.
document.addEventListener('DOMContentLoaded', () => {
    const container = document.querySelector('.ad-details-container');
    if (container) {
        initializeCarouselAndThumbnails(container);
    }
});

// ============== BIDDING ==============

// Function to show toast notifications in the ad details page
// Displays a headMessage and an optional subMessage
// Removes the toast after 5 seconds
function showBiddingToast(headMessage, subMessage = '', type = 'success') {

    if (window.innerWidth <= 768) {
        // On mobile, show instead an alert at the top of the screen

        const alertClass = {
            success: 'alert-success',
            error: 'alert-danger',
            info: 'alert-info',
            warning: 'alert-warning'
        }[type] || 'alert-secondary';

        const alertHtml = `
            <div class="alert ${alertClass} alert-dismissible fade show" role="alert">
                <span class="fw-semibold">${headMessage}</span> <br />
                <span>${subMessage}</span>
                <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
            </div>
        `;

        const alertContainer = document.getElementById('mobileAlertContainer');
        if (!alertContainer) return; // Prevent error if not present on this page
        alertContainer.innerHTML = alertHtml;

        // Remove alert after 5 seconds
        setTimeout(() => {
            const alertEl = alertContainer.querySelector('.alert');
            if (alertEl) {
                const bsAlert = bootstrap.Alert.getOrCreateInstance(alertEl);
                bsAlert.close();
            }
        }, 5000);

        return;
    }

    const toastId = `toast-${Date.now()}`;
    const bgClass = {
        success: 'bg-success text-white',
        error: 'bg-danger text-white',
        info: 'bg-info text-white',
        warning: 'bg-warning text-dark'
    }[type] || 'bg-secondary text-white';

    const toastHtml = `
        <div id="${toastId}" class="toast slide-in ${bgClass}" role="alert" aria-live="assertive" aria-atomic="true">
            <div class="d-flex">
                <div class="toast-body">
                    <div class="fw-bold mb-1">${headMessage}</div>
                    <div class="small">${subMessage}</div>
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Stäng"></button>
            </div>
        </div>
    `;

    const container = document.getElementById('ad-toast-container');
    if (!container) return; // Prevent error if not present on this page
    container.insertAdjacentHTML('beforeend', toastHtml);

    const toastEl = document.getElementById(toastId);
    const bsToast = new bootstrap.Toast(toastEl, {
        delay: 5000,
        autohide: true
    });

    toastEl.addEventListener('hide.bs.toast', () => {
        toastEl.classList.remove('slide-in');
        toastEl.classList.add('slide-out');
    });

    toastEl.addEventListener('hidden.bs.toast', () => {
        toastEl.remove(); // Remove toast from DOM after slide-out finishes
    });

    bsToast.show();
}

// Function that formats price to Swedish locale, removing decimals
function formatPrice(value) {
    return value.toLocaleString('sv-SE', { minimumFractionDigits: 0, maximumFractionDigits: 0 });
}

// Function to update the UI with new bid-data. 
// When a user places a bid, this function is called to update the UI with the new bid information.
function updateBidUI(data) {
    let currentUserId = $("#user-data").data("user-id");
    if (!currentUserId) return;

    let isLeadingBidder = data.leadingBidderId === currentUserId;
    let isPreviousLeadingBidder = data.previousLeadingBidderId === currentUserId;
    let isOutBid = data.outBidUserIds?.includes(currentUserId);
    let bidLeadingStatusHtml = "";

    if (isLeadingBidder) {
        if (isLeadingBidder && data.maxBidAmount > data.currentHighestBid) {
            // User is the leading bidder and has a max bid set above the current leading, visible, bid

            bidLeadingStatusHtml =
                `<div class="fw-semibold">Du har högsta budet!</div>
                    <div class="text-muted small">
                        Nya bud läggs automatiskt åt dig upp till ${formatPrice(data.maxBidAmount)} kr`;
        }
        else {
            // User's max bid is equal to the visible bid, so no automatic bidding up to the max bid amount
            bidLeadingStatusHtml =
                `<div class="fw-semibold">Du har högsta budet!</div>`;
        }
    }
    else if (isOutBid) {
        // User has been outbid

        bidLeadingStatusHtml =
            `<div class="text-danger fw-semibold">Du har blivit överbjuden</div>
                <div class="text-muted small">${data.leadingBidderUserName} leder med ett bud på ${formatPrice(data.currentHighestBid)} kr`;

        if (isPreviousLeadingBidder) {
            // User was the previous leading bidder, so show a toast notification

            showBiddingToast("Du har blivit överbjuden", `${data.leadingBidderUserName} leder med ${formatPrice(data.currentHighestBid)} kr`, 'error');
        }
    }
    else {
        bidLeadingStatusHtml = ""; // User is not involved in the bidding 
    }

    // Update the main view
    $('#bid-leading-status').html(bidLeadingStatusHtml);
    $('#auctionLeadingBid').text(`${formatPrice(data.currentHighestBid)} kr`);
    $('#bid-count').text(data.bidCount);
    $('#bidModalBtnText').text(isLeadingBidder ? "Ändra maxbud" : "Lägg bud");

    // Update the modal where you place a bid
    $('#modal-current-highest-bid').text(`${formatPrice(data.currentHighestBid)} kr`);

    $('#modal-bid-hint').text(isLeadingBidder
        ? `Ditt nuvarande maxbud är ${formatPrice(data.maxBidAmount)} kr`
        : `Lägg ${formatPrice(data.minimumBidAmount)} kr eller mer.`);

    $('#modal-bid-outbid').text(isOutBid ? "Överbjuden" : "");
    $('#PlaceBidBtn').text(isLeadingBidder ? "Bekräfta bud" : "Lägg bud")
}


document.addEventListener("DOMContentLoaded", function () {

    // When the bid-form is submitted, an AJAX request is sent to place a bid.
     $('#bid-form').submit(function (e) {
        e.preventDefault();

        const form = $(this);
        const data = form.serialize();

        // Ajax call to place a bid. Displays appropriate toasts/alerts and updates UI.
        $.ajax({
            url: '/Advertisement/PlaceBid',
            type: 'POST',
            data: data,
            success: function (response) {

                if (response.success) {
                    // If the user is/still is the leading bidder

                    document.activeElement.blur(); // Remove focus from the submit button before closing the modal, for assistive technology users
                    $('#bidModal').modal('hide');

                    showBiddingToast(response.headMessage, response.subMessage, 'success');

                    // Update the UI with the new bid information
                    updateBidUI({
                        currentHighestBid: response.newLeadingVisibleBid,
                        bidCount: response.bidCount,
                        maxBidAmount: response.maxBidAmount, 
                        leadingBidderId: $("#user-data").data("user-id"),
                        previousLeadingBidderId: response.previousLeadingBidderId,
                        outBidUserIds: [],
                        leadingBidderUserName: "",
                        minimumBidAmount: response.minimumBidAmount 
                    });

                    // Update the favourite-icon
                    const adId = $('#adId').val();
                    const $favButton = $(`.favourite-toggle[data-ad-id="${adId}"]`);

                    if (!$favButton.hasClass('favourited')) {
                        $favButton.addClass('favourited');
                    }
                }
                else {
                    // If the user did not become the leading bidder

                    // Update the UI 
                    updateBidUI({
                        currentHighestBid: response.newLeadingVisibleBid,
                        bidCount: response.bidCount,
                        maxBidAmount: response.maxBidAmount ?? null,
                        leadingBidderId: null, // The id is not relevant if the current user is not the leader
                        previousLeadingBidderId: null,
                        outBidUserIds: [],
                        leadingBidderUserName: response.newLeadingBidderUserName ?? '',
                        minimumBidAmount: response.minimumBidAmount ?? null
                    });

                    if (window.innerWidth <= 768) {
                        // If viewed on mobile, display alert inside the modal

                        const $responseDiv = $('#bid-modal-response');

                        $responseDiv.html(
                            `<div class="alert alert-danger alert-dismissable mt-2" role="alert">
                                <strong>${response.headMessage}</strong><br>
                                <small>${response.subMessage}</small>
                            </div>`
                        );
                    }
                    else {
                        showBiddingToast(response.headMessage, response.subMessage, 'error');

                        // Make modal hint text red
                        $('#modal-bid-hint')
                            .removeClass(function (index, className) {
                                return (className.match(/\btext-\S+/g) || []).join(' ');
                            })
                            .addClass('text-danger');
                    }
                }
            },
            error: function () {
                showBiddingToast("Ett fel uppstod vid budgivningen", 'error');
            }
        });

        // Reset styles, alerts and input field when modal is closed
        $('#bidModal').on('hidden.bs.modal', function () {
            // Remove any text-* class from #modal-bid-hint (reset to default)
            $('#modal-bid-hint')
                .removeClass(function (index, className) {
                    return (className.match(/\btext-\S+/g) || []).join(' ');
                });

            // Clear the alert in mobile view
            $('#bid-modal-response').html('');

            // Clear the input field
            $('input[name="maxBidAmount"]').val('');
        });
    });

    const adId = $('#adId').val();
    if (!adId) return; // Prevent error on non-ad pages

    const adGroup = `ad-${adId}`;

    // Initialize SignalR connection for realtime bid updates
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/advertisementhub")
        .build();

    connection.start().then(() => {
        connection.invoke("JoinGroup", adGroup);
    }).catch(err => console.error(err.toString()));

    // When receiving a bid update, update the UI accordingly
    connection.on("ReceiveBidUpdate", function (data) {
        updateBidUI(data);
    });
});

// Show bid history modal when the link is clicked
$(document).on('click', '.show-bid-history', function (e) {
    e.preventDefault();
    const adId = $(this).data('adid');

    $.get('/Advertisement/BidHistoryPartial', { adId: adId }, function (data) {
        $('#bidHistoryModalContent').html(data);
        const modal = new bootstrap.Modal(document.getElementById('bidHistoryModal'));
        modal.show();
    });
});


// ============= Login Page =============
//lift values from Login razor page to Register razor page without a model initiated in Login
document.addEventListener('DOMContentLoaded', function () {
    var registerLink = document.getElementById('register-link');
    var emailInput = document.getElementById('floatingEmail');

    if (registerLink && emailInput) {
        registerLink.addEventListener('click', function (e) {
            e.preventDefault();
            var email = emailInput.value;
            var baseUrl = '/Identity/Account/Register';
            var params = [];
            // Try to get ReturnUrl from a hidden input if present
            var returnUrlInput = document.querySelector('input[name="ReturnUrl"]');
            var returnUrl = returnUrlInput ? returnUrlInput.value : '';
            if (email) params.push('email=' + encodeURIComponent(email));
            if (returnUrl) params.push('returnUrl=' + encodeURIComponent(returnUrl));
            var url = baseUrl + (params.length ? '?' + params.join('&') : '');
            window.location.href = url;
        });
    }
});

//============== TOAST NOTIFICATIONS ==============
/** 
 * Shows a Bootstrap toast in the global #toastContainer.
 * @param {string} message  The body text of the toast.
 * @param {string} [title]  Optional toast header title (defaults to "Notis").
 * @param {number} [delay]  How long to show the toast in ms (defaults to 5000).
 * @param {"info"|"success"|"warning"|"danger"} [type="info"] The type of messages, controls the color and icon.
 */
function showToast(message, title = 'Notis', delay = 5000, type = 'info') {
    // Choose the icon and text color based on the type
    const icons = {
        info: 'bi-info-circle-fill',
        success: 'bi-check-circle-fill',
        warning: 'bi-exclamation-triangle-fill',
        danger: 'bi-x-circle-fill'
    };
    const textClass = (type === 'info' || type === 'warning')
        ? 'text-dark'
        : 'text-white';

    const container = document.getElementById('toastContainer');
    if (!container || !message) return;
    
    // Create the toast element
    const toastEl = document.createElement('div');
    toastEl.className = `toast text-white bg-${type}`;
    toastEl.setAttribute('role', 'alert');
    toastEl.setAttribute('aria-live', 'assertive');
    toastEl.setAttribute('aria-atomic', 'true');
    toastEl.setAttribute('data-bs-delay', delay);

    toastEl.innerHTML = `
        <div class="toast-header bg-${type} ${textClass} border-0">
            <i class="bi ${icons[type]} me-2"></i>
            <strong class="me-auto">${title}</strong>
            <button type="button" class="btn-close ${textClass === 'text-white' ? 'btn-close-white' : ''}" data-bs-dismiss="toast" aria-label="Stäng"></button>
        </div>
        <div class="toast-body ${textClass}">
            ${message}
        </div>
    `;

    // Append and show
    container.appendChild(toastEl);
    const toast = new bootstrap.Toast(toastEl);
    toast.show();

    // Remove from DOM when hidden
    toastEl.addEventListener('hidden.bs.toast', () => toastEl.remove());
}
// Example usage in page-specific scripts or inline:
/*
    document.addEventListener('DOMContentLoaded', () => {
        showToast('Du måste logga in för att spara annonser.', 'Varning', 7000, 'warning');
    });
 */
//================= END TOAST NOTIFICATIONS =================

// ================== SIDEBAR NOTIFICATIONS ==================

document.addEventListener("DOMContentLoaded", () => {
    // Get the elements for the notification icon, dot and sidebar
    const notificationDotDesktop = document.getElementById("notification-dot-desktop");
    const notificationDotMobile = document.getElementById("notification-dot-mobile");
    const sidebarContainer = document.getElementById("notification-sidebar-container");
    const notificationIcon = document.getElementById("notification-icon");

    // On load, check if there are any unread notifications and show the dot by the icon if so
    fetch('/Home/HasUnreadNotifications')
        .then(response => response.json())
        .then(data => {
            if (data.hasUnread) {
                if (notificationDotDesktop) notificationDotDesktop.style.display = "inline-block";
                if (notificationDotMobile) notificationDotMobile.style.display = "inline-block";
            }
        })
        .catch(error => console.error("Kunde inte hämta info om olästa notiser:", error));


    // Initialize SignalR connection for real-time notifications
    // Used to show the notification dot by the icon when a new notification arrives
    if (typeof signalR !== 'undefined') {
        const connection = new signalR.HubConnectionBuilder()
            .withUrl("/notificationHub")
            .build();

        connection.on("ReceiveNotification", () => {
            if (notificationDotDesktop) notificationDotDesktop.style.display = "inline-block";
            if (notificationDotMobile) notificationDotMobile.style.display = "inline-block";
        });

        connection.start().catch(err => console.error("SignalR connection failed:", err));
    }

    // If the icon or sidebar container is not present, exit early
    if (!notificationIcon || !sidebarContainer) return;

    // Variable to track if the offcanvas listener has already been added, to prevent duplicates
    let offcanvasListenerAdded = false;

    // When the notification icon is clicked, fetch the sidebar content and show it
    notificationIcon.addEventListener("click", function () {
        fetch("/Home/LoadNotificationSidebar")
            .then(response => {
                if (!response.ok) throw new Error("Nätverksfel vid hämtning av notiser.");
                return response.text();
            })
            .then(html => {
                sidebarContainer.innerHTML = html;

                // Wait for the DOM to update before showing the offcanvas
                setTimeout(() => {
                    const offcanvasEl = document.getElementById("notificationSidebar");
                    if (!offcanvasEl) {
                        console.error("Kunde inte hitta #notificationSidebar i AJAX-svar.");
                        return;
                    }

                    // Initialize the Bootstrap Offcanvas component
                    const bsOffcanvas = new bootstrap.Offcanvas(offcanvasEl);

                    // If the offcanvas listener has not been added yet, add it
                    if (!offcanvasListenerAdded) {

                        // Add event listener to mark all notifications as read when the sidebar is shown
                        offcanvasEl.addEventListener('shown.bs.offcanvas', () => {
                            if (notificationDotDesktop) notificationDotDesktop.style.display = "none";
                            if (notificationDotMobile) notificationDotMobile.style.display = "none";

                            fetch('/Home/MarkAllAsRead', { method: 'POST' })
                                .then(response => {
                                    if (!response.ok) throw new Error("Kunde inte markera notiser som lästa.");
                                })
                                .catch(err => console.error("Fel vid markering av notiser som lästa:", err));
                        });
                        offcanvasListenerAdded = true;
                    }

                    bsOffcanvas.show();
                }, 0);
            })
            .catch(error => {
                console.error("Kunde inte ladda aviseringar:", error);
            });
    });

    // Check for expiring ads every 24 hours (the controller method handles the logic))
    (async () => {
        try {
            const response = await fetch('/Advertisement/CheckAdsExpiringToday');
            const result = await response.json();

            if (result.success && result.created) {
                console.log("New expiring ad notifications created.");
            }
        } catch (error) {
            console.error("Failed to check expiring ads:", error);
        }
    })();
});

// ================== END SIDEBAR NOTIFICATIONS ==================
