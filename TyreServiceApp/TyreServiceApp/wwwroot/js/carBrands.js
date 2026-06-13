document.addEventListener('DOMContentLoaded', function () {
    var data = {
        "LADA (ВАЗ)": ["Granta", "Vesta", "Niva", "Niva Travel", "Largus", "XRAY", "Kalina", "Priora", "2107", "2114", "2110", "2109", "2121", "2131"],
        "Toyota": ["Camry", "Corolla", "RAV4", "Land Cruiser", "Land Cruiser Prado", "Highlander", "Fortuner", "Hilux", "Yaris", "C-HR", "Avensis", "Verso", "Alphard", "Crown", "Mark X", "Supra"],
        "Hyundai": ["Solaris", "Elantra", "Tucson", "Santa Fe", "Creta", "Sonata", "ix35", "Getz", "Accent", "Palisade", "Kona", "Staria", "Porter"],
        "Kia": ["Rio", "Sportage", "Sorento", "Optima", "Ceed", "Cerato", "Soul", "Mohave", "Stinger", "Picanto", "Carnival", "K5", "Seltos"],
        "Renault": ["Logan", "Sandero", "Duster", "Kaptur", "Arkana", "Megan", "Fluence", "Koleos", "Laguna", "Scenic", "Master"],
        "Nissan": ["Qashqai", "X-Trail", "Murano", "Juke", "Pathfinder", "Patrol", "Navara", "Teana", "Almera", "Note", "Micra", "Leaf", "GT-R"],
        "Volkswagen": ["Polo", "Passat", "Tiguan", "Touareg", "Golf", "Jetta", "Teramont", "Caddy", "Transporter", "Amarok", "ID.4", "Taos"],
        "BMW": ["X3", "X5", "X1", "X6", "3 Series", "5 Series", "7 Series", "X4", "X7", "M5", "M3", "1 Series", "iX", "i4"],
        "Mercedes-Benz": ["E-Class", "C-Class", "S-Class", "GLC", "GLE", "GLS", "A-Class", "GLK", "ML", "G-Class", "Sprinter", "Vito", "EQS", "EQE"],
        "Audi": ["Q5", "Q7", "A4", "A6", "Q3", "A3", "A8", "Q8", "TT", "R8", "e-tron", "Q2", "A5", "A7"],
        "Ford": ["Focus", "Kuga", "Explorer", "Escape", "Mondeo", "Fiesta", "Fusion", "Ranger", "Transit", "Mustang", "Edge", "Everest"],
        "Chevrolet": ["Cruze", "Lacetti", "Aveo", "Niva", "Captiva", "Tahoe", "Malibu", "Spark", "TrailBlazer", "Camaro", "Suburban", "Traverse"],
        "Mazda": ["CX-5", "CX-7", "3", "6", "CX-9", "CX-30", "MX-5", "RX-8", "CX-3", "2", "CX-60"],
        "Mitsubishi": ["Outlander", "Pajero", "L200", "ASX", "Lancer", "Eclipse Cross", "Montero", "i-MiEV", "Mirage", "Delica"],
        "Subaru": ["Forester", "Outback", "Impreza", "Legacy", "XV", "WRX", "Tribeca", "BRZ", "Ascent", "Leone"],
        "Skoda": ["Octavia", "Rapid", "Kodiaq", "Fabia", "Yeti", "Superb", "Karoq", "Kamiq", "Enyaq", "Scala"],
        "Volvo": ["XC60", "XC90", "XC40", "S60", "S90", "V60", "V90", "S40", "C40", "EX30", "EX90"],
        "Lexus": ["RX", "NX", "LX", "ES", "IS", "GX", "UX", "LS", "RC", "LC"],
        "Chery": ["Tiggo 7", "Tiggo 8", "Tiggo 4", "Arrizo 8", "Tiggo 5", "Arrizo 5", "Tiggo 9", "eQ1"],
        "Geely": ["Coolray", "Atlas", "Monjaro", "Tugella", "Emgrand", "Preface", "Okavango", "Geometry C"],
        "Haval": ["Jolion", "F7", "H6", "F7x", "Dargo", "H9", "M6", "H2"],
        "BYD": ["Tang", "Han", "Atto 3", "Seal", "Dolphin", "Song Plus", "Yuan Plus", "Qin Plus"],
        "Changan": ["CS35", "CS55", "CS75", "UNI-K", "UNI-T", "Eado", "Alsvin", "Raeton"]
    };

    var brandDatalist = document.getElementById('brandList');
    if (brandDatalist) {
        for (var b in data) {
            var opt = document.createElement('option');
            opt.value = b;
            brandDatalist.appendChild(opt);
        }
    }

    var brandInput = document.querySelector('[name="Brand"]');
    var modelInput = document.getElementById('Model');
    var modelDatalist = document.getElementById('modelList');

    function fillModels(brand) {
        if (!modelDatalist) return;
        modelDatalist.innerHTML = '';
        var list = data[brand];
        if (list) {
            list.forEach(function (m) {
                var opt = document.createElement('option');
                opt.value = m;
                modelDatalist.appendChild(opt);
            });
            if (modelInput) modelInput.placeholder = 'Выберите модель';
        } else {
            for (var b in data) {
                data[b].forEach(function (m) {
                    var opt = document.createElement('option');
                    opt.value = m;
                    modelDatalist.appendChild(opt);
                });
            }
            if (modelInput) modelInput.placeholder = 'Введите модель';
        }
    }

    if (brandInput) {
        brandInput.addEventListener('change', function () {
            fillModels(this.value);
        });
    }

    fillModels(brandInput ? brandInput.value : '');
});
