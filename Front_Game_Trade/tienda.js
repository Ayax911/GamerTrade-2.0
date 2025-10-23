// 🕹 Lista de productos
const productos = [
  { 
    nombre: "Final Fantasy VII Rebirth", 
    precio: 40,  
    categoria: "aventura", 
    imagen: "Imagenes/img1.jpg",
    descripcion: "Final Fantasy VII Rebirth es un videojuego de acción RPG a cargo de Square Enix para PlayStation 5 y PC que continúa con la historia de Final Fantasy VII Remake. La travesía desconocida continúa..."
  },
  { 
    nombre: "Split Fiction", 
    precio: 30,  
    categoria: "estrategia", 
    imagen: "Imagenes/img2.jpg",
    descripcion: "Un juego de estrategia con decisiones narrativas que cambian el curso de la historia."
  },
  { 
    nombre: "Civilization VII", 
    precio: 45,  
    categoria: "estrategia", 
    imagen: "Imagenes/img3.jpg",
    descripcion: "Lidera tu civilización a través de los siglos y conquista el mundo con sabiduría y estrategia."
  },
  { 
    nombre: "The Last of Us", 
    precio: 70,  
    categoria: "aventura", 
    imagen: "Imagenes/img4.jpg",
    descripcion: "Sobrevive en un mundo post-apocalíptico lleno de peligros y emociones intensas."
  },
  { 
    nombre: "Monster Hunter", 
    precio: 55,  
    categoria: "rol", 
    imagen: "Imagenes/img5.jpg",
    descripcion: "Embárcate en una cacería épica, derrota criaturas colosales y crea poderosas armas."
  },
];

// 🎨 Elementos del DOM
const contenedor = document.getElementById("productos");
const filtroCategoria = document.getElementById("categoria");

// 💡 Función para mostrar productos
function mostrarProductos(lista) {
  contenedor.innerHTML = "";
  lista.forEach((p, i) => {
    const card = document.createElement("div");
    card.classList.add("product-card");
    card.innerHTML = `
      <img src="${p.imagen}" alt="${p.nombre}" id="img${i + 1}" class="img-juego"
           data-nombre="${p.nombre}"
           data-precio="$${p.precio} COP"
           data-categoria="${p.categoria}"
           data-descripcion="${p.descripcion}"
           data-imagen="${p.imagen}">
      <h3>${p.nombre}</h3>
      <p>Precio: $${p.precio} COP</p>
    `;
    contenedor.appendChild(card);
  });

  // 📌 Asignar evento de clic a cada imagen
  document.querySelectorAll(".img-juego").forEach((img) => {
    img.addEventListener("click", () => {
      const params = new URLSearchParams({
        nombre: img.dataset.nombre,
        precio: img.dataset.precio,
        categoria: img.dataset.categoria,
        descripcion: img.dataset.descripcion,
        imagen: img.dataset.imagen
      });

      // Abrir la página del juego
      window.location.href = `game.html?${params.toString()}`;
    });
  });
}

// Mostrar todos los productos al cargar
mostrarProductos(productos);

// 🧩 Filtro por categoría
filtroCategoria.addEventListener("change", () => {
  const categoria = filtroCategoria.value;
  const filtrados = categoria === "todos" 
    ? productos 
    : productos.filter(p => p.categoria === categoria);
  mostrarProductos(filtrados);
});
