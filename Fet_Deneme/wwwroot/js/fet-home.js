// SweetAlert2 CDN yüklemesi için HTML'e script ekleyin veya burada import edin
// Bu dosya home ekranı için özel JS içerir

function showNewProjectModal() {
    Swal.fire({
        title: 'Yeni Proje',
        text: 'Yeni bir FET projesi başlatmak istiyor musunuz?',
        icon: 'question',
        showCancelButton: true,
        confirmButtonText: 'Evet',
        cancelButtonText: 'Hayır'
    }).then((result) => {
        if (result.isConfirmed) {
            // Yeni proje başlatma işlemi burada yapılacak
            window.location.href = '/Home/NewProject';
        }
    });
}

function showOpenProjectModal() {
    Swal.fire({
        title: 'Proje Aç',
        html: '<input type="file" id="fetFileInput" accept=".fet" class="swal2-input" />',
        showCancelButton: true,
        confirmButtonText: 'Aç',
        cancelButtonText: 'İptal',
        preConfirm: () => {
            const fileInput = Swal.getPopup().querySelector('#fetFileInput');
            if (!fileInput.files[0]) {
                Swal.showValidationMessage('Lütfen bir .fet dosyası seçin');
            }
            return fileInput.files[0];
        }
    }).then((result) => {
        if (result.isConfirmed && result.value) {
            // Dosya okuma işlemi burada yapılacak
            const file = result.value;
            // Örnek: dosyayı bir form ile sunucuya gönderebilirsiniz
            // veya JS ile içeriğini okuyabilirsiniz
        }
    });
}
