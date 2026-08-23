document.addEventListener('DOMContentLoaded',()=>document.querySelectorAll('.cart-item .quantity-input').forEach(input=>input.addEventListener('change',()=>{if(+input.value<1)input.value=1})));
