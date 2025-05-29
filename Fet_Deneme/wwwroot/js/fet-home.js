// SweetAlert2 CDN yüklemesi için HTML'e script ekleyin veya burada import edin
// Bu dosya home ekranı için özel JS içerir

function showNewProjectModal() {
  Swal.fire({
    title: "Yeni Proje",
    text: "Yeni bir FET projesi başlatmak istiyor musunuz?",
    icon: "question",
    showCancelButton: true,
    confirmButtonText: "Evet",
    cancelButtonText: "Hayır",
  }).then((result) => {
    if (result.isConfirmed) {
      // Yeni proje başlatma işlemi burada yapılacak
      window.location.href = "/Home/NewProject";
    }
  });
}

function showOpenProjectModal() {
  Swal.fire({
    title: "Proje Aç",
    html: '<input type="file" id="fetFileInput" accept=".fet" class="swal2-input" />',
    showCancelButton: true,
    confirmButtonText: "Aç",
    cancelButtonText: "İptal",
    preConfirm: () => {
      const fileInput = Swal.getPopup().querySelector("#fetFileInput");
      if (!fileInput.files[0]) {
        Swal.showValidationMessage("Lütfen bir .fet dosyası seçin");
      }
      return fileInput.files[0];
    },
  }).then((result) => {
    if (result.isConfirmed && result.value) {
      // Dosya okuma işlemi burada yapılacak
      const file = result.value;
      // Örnek: dosyayı bir form ile sunucuya gönderebilirsiniz
      // veya JS ile içeriğini okuyabilirsiniz
    }
  });
}

window.fetCurrentFileName = null;
window.fetCurrentXmlContent = null;

window.updateFileBar = function () {
  if (window.fetCurrentFileName) {
    document.getElementById("fet-filename-bar").style.display = "";
    document.getElementById("fet-current-filename").textContent =
      window.fetCurrentFileName;
  } else {
    document.getElementById("fet-filename-bar").style.display = "none";
  }
};

window.saveToLocal = function () {
  if (window.fetCurrentFileName && window.fetCurrentXmlContent) {
    localStorage.setItem("fetFileName", window.fetCurrentFileName);
    localStorage.setItem("fetXmlContent", window.fetCurrentXmlContent);
  } else {
    localStorage.removeItem("fetFileName");
    localStorage.removeItem("fetXmlContent");
  }
};

window.loadFromLocal = function () {
  const fileName = localStorage.getItem("fetFileName");
  const xmlContent = localStorage.getItem("fetXmlContent");
  if (fileName && xmlContent) {
    window.fetCurrentFileName = fileName;
    window.fetCurrentXmlContent = xmlContent;
    // Sunucuya da yükle (Aç mantığıyla)
    fetch("/Home/OpenProject?fileName=" + encodeURIComponent(fileName), {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(xmlContent),
    })
      .then((r) => r.json())
      .then((data) => {
        if (data.success) {
          window.updateFileBar();
        }
      });
  }
};

window.fetNewProject = function () {
  fetch("/Home/NewProject", { method: "POST" })
    .then((r) => r.json())
    .then((data) => {
      if (data.success) {
        window.fetCurrentFileName = data.fileName;
        window.fetCurrentXmlContent = data.xmlContent || null;
        window.updateFileBar();
        window.saveToLocal();
        Swal.fire("Yeni proje oluşturuldu", "", "success");
      }
    });
};

window.fetOpenProject = function () {
  Swal.fire({
    title: "Proje Aç",
    html: '<input type="file" id="fetFileInput" accept=".fet,.xml" class="swal2-input" />',
    showCancelButton: true,
    confirmButtonText: "Aç",
    cancelButtonText: "İptal",
    preConfirm: () => {
      const fileInput = Swal.getPopup().querySelector("#fetFileInput");
      if (!fileInput.files[0]) {
        Swal.showValidationMessage("Lütfen bir .fet dosyası seçin");
      }
      return fileInput.files[0];
    },
  }).then((result) => {
    if (result.isConfirmed && result.value) {
      const file = result.value;
      const reader = new FileReader();
      reader.onload = function (e) {
        window.fetCurrentXmlContent = e.target.result;
        window.fetCurrentFileName = file.name;
        // Browser ortamında file.path yoktur, ama varsa backend'e ilet
        let filePath = file.path || "";
        fetch(
          "/Home/OpenProject?fileName=" +
            encodeURIComponent(file.name) +
            "&filePath=" +
            encodeURIComponent(filePath),
          {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(window.fetCurrentXmlContent),
          }
        )
          .then((r) => r.json())
          .then((data) => {
            if (data.success) {
              window.updateFileBar();
              window.saveToLocal();
              Swal.fire("Proje açıldı", "", "success");
            }
          });
      };
      reader.readAsText(file);
    }
  });
};

document.getElementById("fet-btn-save").onclick = function () {
  if (!window.fetCurrentFileName) return;
  fetch("/Home/SaveProject", { method: "POST" })
    .then((r) => r.json())
    .then((data) => {
      if (data.success) {
        Swal.fire("Kaydedildi", "", "success");
        window.saveToLocal();
      }
    });
};

document.getElementById("fet-btn-saveas").onclick = function () {
  if (!window.fetCurrentFileName) return;
  if (window.showSaveFilePicker) {
    (async () => {
      try {
        const opts = {
          suggestedName: window.fetCurrentFileName,
          types: [
            {
              description: "FET XML",
              accept: { "application/xml": [".fet", ".xml"] },
            },
          ],
        };
        const handle = await window.showSaveFilePicker(opts);
        const writable = await handle.createWritable();
        await writable.write(window.fetCurrentXmlContent || "");
        await writable.close();
        window.fetCurrentFileName = handle.name;
        window.updateFileBar();
        window.saveToLocal();
        Swal.fire("Farklı kaydedildi", "", "success");
      } catch (e) {
        Swal.fire("İşlem iptal edildi", "", "info");
      }
    })();
  } else {
    Swal.fire({
      title: "Farklı Kaydet",
      input: "text",
      inputLabel: "Kaydetmek istediğiniz tam dosya yolu",
      inputValue: window.fetCurrentFileName,
      showCancelButton: true,
      confirmButtonText: "Kaydet",
      cancelButtonText: "İptal",
      inputPlaceholder: "C:\\Users\\kullanici\\Desktop\\dosya.fet",
    }).then((result) => {
      if (result.isConfirmed && result.value) {
        fetch("/Home/SaveProjectAs", {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({
            xmlContent: window.fetCurrentXmlContent || "",
            filePath: result.value,
          }),
        })
          .then((r) => r.json())
          .then((data) => {
            if (data.success) {
              window.fetCurrentFileName = data.fileName;
              window.updateFileBar();
              window.saveToLocal();
              Swal.fire("Farklı kaydedildi", "", "success");
            } else {
              Swal.fire("Hata", data.message || "Kaydedilemedi", "error");
            }
          });
      }
    });
  }
};

window.fetImportSubjectsCsv = function () {
  Swal.fire({
    title: "Ders adlarını CSV dosyasından al",
    html: '<input type="file" id="fetCsvInput" accept=".csv,text/csv" class="swal2-input" />',
    showCancelButton: true,
    confirmButtonText: "İçe Aktar",
    cancelButtonText: "İptal",
    preConfirm: () => {
      const fileInput = Swal.getPopup().querySelector("#fetCsvInput");
      if (!fileInput.files[0]) {
        Swal.showValidationMessage("Lütfen bir CSV dosyası seçin");
      }
      return fileInput.files[0];
    },
  }).then((result) => {
    if (result.isConfirmed && result.value) {
      const file = result.value;
      const reader = new FileReader();
      reader.onload = function (e) {
        const csvContent = e.target.result;
        fetch("/Home/ImportSubjectsCsv", {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({
            csvContent: csvContent,
            fileName: window.fetCurrentFileName || "",
          }),
        })
          .then((r) => r.json())
          .then((data) => {
            if (data.success) {
              window.fetCurrentXmlContent = data.xmlContent;
              window.saveToLocal();
              Swal.fire(
                "Dersler başarıyla içe aktarıldı",
                `${data.added} ders eklendi.`,
                "success"
              );
            } else {
              Swal.fire("Hata", data.message || "İçe aktarılamadı", "error");
            }
          });
      };
      reader.readAsText(file);
    }
  });
};

window.fetImportTeachersCsv = function () {
  Swal.fire({
    title: "Öğretmen adlarını CSV dosyasından al",
    html: '<input type="file" id="fetCsvInputTeacher" accept=".csv,text/csv" class="swal2-input" />',
    showCancelButton: true,
    confirmButtonText: "İçe Aktar",
    cancelButtonText: "İptal",
    preConfirm: () => {
      const fileInput = Swal.getPopup().querySelector("#fetCsvInputTeacher");
      if (!fileInput.files[0]) {
        Swal.showValidationMessage("Lütfen bir CSV dosyası seçin");
      }
      return fileInput.files[0];
    },
  }).then((result) => {
    if (result.isConfirmed && result.value) {
      const file = result.value;
      const reader = new FileReader();
      reader.onload = function (e) {
        const csvContent = e.target.result;
        fetch("/Home/ImportTeachersCsv", {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({
            csvContent: csvContent,
            fileName: window.fetCurrentFileName || "",
          }),
        })
          .then((r) => r.json())
          .then((data) => {
            if (data.success) {
              window.fetCurrentXmlContent = data.xmlContent;
              window.saveToLocal();
              Swal.fire(
                "Öğretmenler başarıyla içe aktarıldı",
                `${data.added} öğretmen eklendi.`,
                "success"
              );
            } else {
              Swal.fire("Hata", data.message || "İçe aktarılamadı", "error");
            }
          });
      };
      reader.readAsText(file);
    }
  });
};

window.fetImportStudentsCsv = function () {
  Swal.fire({
    title: "Öğrenci (Yıl/Grup/Altgrup) adlarını CSV dosyasından al",
    html: '<input type="file" id="fetCsvInputStudents" accept=".csv,text/csv" class="swal2-input" />',
    showCancelButton: true,
    confirmButtonText: "İçe Aktar",
    cancelButtonText: "İptal",
    preConfirm: () => {
      const fileInput = Swal.getPopup().querySelector("#fetCsvInputStudents");
      if (!fileInput.files[0]) {
        Swal.showValidationMessage("Lütfen bir CSV dosyası seçin");
      }
      return fileInput.files[0];
    },
  }).then((result) => {
    if (result.isConfirmed && result.value) {
      const file = result.value;
      const reader = new FileReader();
      reader.onload = function (e) {
        const csvContent = e.target.result;
        fetch("/Home/ImportStudentsCsv", {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({
            csvContent: csvContent,
            fileName: window.fetCurrentFileName || "",
          }),
        })
          .then((r) => r.json())
          .then((data) => {
            if (data.success) {
              window.fetCurrentXmlContent = data.xmlContent;
              window.saveToLocal();
              Swal.fire(
                "Öğrenciler başarıyla içe aktarıldı",
                `${data.added} öğrenci/yıl/grup/altgrup eklendi.`,
                "success"
              );
            } else {
              Swal.fire("Hata", data.message || "İçe aktarılamadı", "error");
            }
          });
      };
      reader.readAsText(file);
    }
  });
};

window.fetImportActivitiesCsv = function () {
  Swal.fire({
    title: "Etkinlikleri (Activities) CSV dosyasından al",
    html: '<input type="file" id="fetCsvInputActivities" accept=".csv,text/csv" class="swal2-input" />',
    showCancelButton: true,
    confirmButtonText: "İçe Aktar",
    cancelButtonText: "İptal",
    preConfirm: () => {
      const fileInput = Swal.getPopup().querySelector("#fetCsvInputActivities");
      if (!fileInput.files[0]) {
        Swal.showValidationMessage("Lütfen bir CSV dosyası seçin");
      }
      return fileInput.files[0];
    },
  }).then((result) => {
    if (result.isConfirmed && result.value) {
      const file = result.value;
      const reader = new FileReader();
      reader.onload = function (e) {
        const csvContent = e.target.result;
        fetch("/Home/ImportActivitiesCsv", {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({
            csvContent: csvContent,
            fileName: window.fetCurrentFileName || "",
          }),
        })
          .then((r) => r.json())
          .then((data) => {
            if (data.success) {
              window.fetCurrentXmlContent = data.xmlContent;
              window.saveToLocal();
              Swal.fire(
                "Etkinlikler başarıyla içe aktarıldı",
                `${data.added} etkinlik eklendi.`,
                "success"
              );
            } else {
              Swal.fire("Hata", data.message || "İçe aktarılamadı", "error");
            }
          });
      };
      reader.readAsText(file);
    }
  });
};

window.fetImportMultipleCsv = function () {
  Swal.fire({
    title: "Çoklu CSV İçe Aktar",
    html: '<input type="file" id="fetCsvInputMulti" accept=".csv,text/csv" class="swal2-input" multiple />',
    showCancelButton: true,
    confirmButtonText: "İçe Aktar",
    cancelButtonText: "İptal",
    preConfirm: () => {
      const fileInput = Swal.getPopup().querySelector("#fetCsvInputMulti");
      if (!fileInput.files.length) {
        Swal.showValidationMessage("Lütfen en az bir CSV dosyası seçin");
      }
      return fileInput.files;
    },
  }).then((result) => {
    if (result.isConfirmed && result.value) {
      const files = Array.from(result.value);
      const fileMap = {};
      files.forEach((f) => (fileMap[f.name.toLowerCase()] = f));
      const importOrder = [
        {
          name: "subjects.csv",
          endpoint: "/Home/ImportSubjectsCsv",
          label: "Dersler",
        },
        {
          name: "teachers.csv",
          endpoint: "/Home/ImportTeachersCsv",
          label: "Öğretmenler",
        },
        {
          name: "student.csv",
          endpoint: "/Home/ImportStudentsCsv",
          label: "Öğrenciler",
        },
        {
          name: "activities.csv",
          endpoint: "/Home/ImportActivitiesCsv",
          label: "Etkinlikler",
        },
      ];
      let results = [];
      let lastXml = window.fetCurrentXmlContent;
      let lastFileName = window.fetCurrentFileName || "";
      let chain = Promise.resolve();
      importOrder.forEach((io) => {
        if (fileMap[io.name]) {
          chain = chain.then(
            () =>
              new Promise((resolve) => {
                const reader = new FileReader();
                reader.onload = function (e) {
                  fetch(io.endpoint, {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({
                      csvContent: e.target.result,
                      fileName: lastFileName,
                    }),
                  })
                    .then((r) => r.json())
                    .then((data) => {
                      if (data.success) {
                        lastXml = data.xmlContent;
                        window.fetCurrentXmlContent = lastXml;
                        window.saveToLocal();
                        results.push(
                          io.label + " ✓ (" + (data.added || 0) + " eklendi)"
                        );
                      } else {
                        results.push(
                          io.label +
                            " × (Hata: " +
                            (data.message || "İçe aktarılamadı") +
                            ")"
                        );
                      }
                      resolve();
                    });
                };
                reader.readAsText(fileMap[io.name]);
              })
          );
        }
      });
      chain.then(() => {
        Swal.fire("Çoklu İçe Aktar Sonucu", results.join("<br>"), "info");
      });
    }
  });
};

window.addEventListener("DOMContentLoaded", function () {
  window.loadFromLocal();
});

window.updateFileBar();

window.fetShowData = function (dataType) {
  if (!window.fetCurrentFileName) {
    Swal.fire("Hata", "Önce bir proje açın veya oluşturun", "error");
    return;
  }
  fetch("/Home/ShowData?dataType=" + dataType)
    .then((r) => r.json())
    .then((data) => {
      if (data.success) {
        let title = "";
        let htmlContent = "";
        switch (dataType) {
          case "subjects":
            title = "Ders Adları";
            if (data.data.length === 0) {
              htmlContent =
                '<div class="alert alert-info">Henüz ders tanımlanmamış.</div>';
            } else {
              htmlContent =
                '<table class="table table-striped table-sm"><thead><tr><th>Adı</th><th>Uzun Adı</th><th>Kod</th><th>Açıklama</th></tr></thead><tbody>';
              data.data.forEach((subject) => {
                htmlContent += `<tr><td>${subject.name || ""}</td><td>${
                  subject.longName || ""
                }</td><td>${subject.code || ""}</td><td>${
                  subject.comments || ""
                }</td></tr>`;
              });
              htmlContent += "</tbody></table>";
            }
            break;
          case "teachers":
            title = "Öğretmenler";
            if (data.data.length === 0) {
              htmlContent =
                '<div class="alert alert-info">Henüz öğretmen tanımlanmamış.</div>';
            } else {
              htmlContent =
                '<table class="table table-striped table-sm"><thead><tr><th>Adı</th><th>Hedef Ders Saati</th><th>Açıklama</th></tr></thead><tbody>';
              data.data.forEach((teacher) => {
                htmlContent += `<tr><td>${teacher.name || ""}</td><td>${
                  teacher.targetNumberOfHours || "0"
                }</td><td>${teacher.comments || ""}</td></tr>`;
              });
              htmlContent += "</tbody></table>";
            }
            break;
          case "students":
            title = "Öğrenciler (Yıl/Grup/Altgruplar)";
            if (data.data.length === 0) {
              htmlContent =
                '<div class="alert alert-info">Henüz öğrenci grupları tanımlanmamış.</div>';
            } else {
              htmlContent = '<ul class="list-group">';
              data.data.forEach((year) => {
                htmlContent += `<li class="list-group-item">
                                    <strong>Yıl: ${year.name}</strong> (${
                  year.numberOfStudents
                } öğrenci)
                                    ${
                                      year.groups && year.groups.length > 0
                                        ? '<ul class="list-group mt-2">'
                                        : ""
                                    }`;
                if (year.groups && year.groups.length > 0) {
                  year.groups.forEach((group) => {
                    htmlContent += `<li class="list-group-item">
                                            <strong>Grup: ${
                                              group.name
                                            }</strong> (${
                      group.numberOfStudents
                    } öğrenci)
                                            ${
                                              group.subgroups &&
                                              group.subgroups.length > 0
                                                ? '<ul class="list-group mt-2">'
                                                : ""
                                            }`;
                    if (group.subgroups && group.subgroups.length > 0) {
                      group.subgroups.forEach((subgroup) => {
                        htmlContent += `<li class="list-group-item">
                                                    <strong>Altgrup: ${subgroup.name}</strong> (${subgroup.numberOfStudents} öğrenci)
                                                </li>`;
                      });
                      htmlContent += "</ul>";
                    }
                    htmlContent += "</li>";
                  });
                  htmlContent += "</ul>";
                }
                htmlContent += "</li>";
              });
              htmlContent += "</ul>";
            }
            break;
          case "activities":
            title = "Etkinlikler";
            if (data.data.length === 0) {
              htmlContent =
                '<div class="alert alert-info">Henüz etkinlik tanımlanmamış.</div>';
            } else {
              htmlContent =
                '<table class="table table-striped table-sm"><thead><tr><th>ID</th><th>Öğretmen</th><th>Ders</th><th>Öğrenci Sayısı</th><th>Süre</th><th>Durum</th></tr></thead><tbody>';
              data.data.forEach((act) => {
                let ogrCell =
                  act.studentCount && act.studentCount > 0
                    ? act.studentCount
                    : "0";
                htmlContent += `<tr>
                                    <td>${act.id}</td>
                                    <td>${act.teacher || ""}</td>
                                    <td>${act.subject || ""}</td>
                                    <td>${ogrCell}</td>
                                    <td>${act.duration}/${
                  act.totalDuration || act.duration
                }</td>
                                    <td>${
                                      act.active
                                        ? '<span class="badge bg-success">Aktif</span>'
                                        : '<span class="badge bg-danger">Pasif</span>'
                                    }</td>
                                </tr>`;
              });
              htmlContent += "</tbody></table>";
            }
            break;
        }
        Swal.fire({
          title: title,
          html: htmlContent,
          width: "800px",
          heightAuto: false,
          showConfirmButton: true,
          confirmButtonText: "Kapat",
          customClass: {
            popup: "swal-wide",
            htmlContainer: "swal-scrollable",
            content: "text-start",
          },
        });
      } else {
        Swal.fire("Hata", data.message || "Veriler gösterilemiyor", "error");
      }
    })
    .catch((err) => {
      console.error(err);
      Swal.fire(
        "Sunucu Hatası",
        "Veri gösterme işlemi başarısız: " + err.message,
        "error"
      );
    });
};

window.fetShowDays = function () {
  if (!window.fetCurrentFileName) {
    Swal.fire("Hata", "Önce bir proje açın veya oluşturun", "error");
    return;
  }
  fetch("/Home/ShowData?dataType=days")
    .then((r) => r.json())
    .then((data) => {
      let items = data.success && data.data ? data.data : [];
      let html = `
<div class="mb-2 d-flex align-items-center">
    <label class="me-2">Days:</label>
    <input type="number" class="form-control form-control-sm" style="width:90px;" min="1" max="14" value="${
      items.length
    }" id="fet-spin-main-days" />
</div>
<ul class="list-group mb-2" id="fet-list-main-days" style="height:220px;overflow:auto;">
    ${items
      .map(
        (item, i) =>
          `<li class="list-group-item fet-selectable text-start" data-idx="${i}">${
            i + 1
          }. N: ${item.name || ""}, LN: ${
            item.longName || item.name || ""
          }</li>`
      )
      .join("")}
</ul>
<div class="d-flex gap-2">
    <button class="btn btn-outline-success btn-sm w-100" id="fet-btn-add-day">Insert</button>
    <button class="btn btn-outline-warning btn-sm w-100" id="fet-btn-edit-day">Düzenle</button>
    <button class="btn btn-outline-danger btn-sm w-100" id="fet-btn-del-day">Kaldır</button>
</div>`;
      Swal.fire({
        title: "Haftanın günleri",
        html: html,
        width: "40%",
        heightAuto: false,
        showConfirmButton: false,
        customClass: {
          popup: "swal-wide",
          htmlContainer: "swal-scrollable",
          content: "text-start",
        },
        didOpen: () => {
          let selectedIdx = 0;
          const list = document.getElementById("fet-list-main-days");
          if (list) {
            const itemsEls = list.querySelectorAll(".fet-selectable");
            if (itemsEls.length > 0) itemsEls[0].classList.add("active");
            itemsEls.forEach((el) => {
              el.onclick = function () {
                itemsEls.forEach((e) => e.classList.remove("active"));
                this.classList.add("active");
                selectedIdx = parseInt(this.getAttribute("data-idx"));
              };
            });
          }
          // Number input değişimi
          document.getElementById("fet-spin-main-days").onchange = function () {
            let newCount = parseInt(this.value);
            let diff = newCount - items.length;
            if (diff > 0) {
              // Ekle
              let lastNum = items.length;
              let addNext = (idx) => {
                if (idx <= 0) {
                  window.fetShowDays();
                  return;
                }
                // FET algoritması: D{n+1}, Day {n+1}
                let newName = `D${lastNum + 1}`;
                let newLongName = `Day ${lastNum + 1}`;
                fetch("/Home/AddDay", {
                  method: "POST",
                  headers: { "Content-Type": "application/json" },
                  body: JSON.stringify({
                    Name: newName,
                    LongName: newLongName,
                  }),
                })
                  .then((r) => r.json())
                  .then((j) => {
                    lastNum++;
                    addNext(idx - 1);
                  });
              };
              addNext(diff);
            } else if (diff < 0) {
              // Sil
              let delNext = (idx) => {
                if (idx <= 0) {
                  window.fetShowDays();
                  return;
                }
                fetch("/Home/DeleteDay", {
                  method: "POST",
                  headers: { "Content-Type": "application/json" },
                  body: JSON.stringify({ Index: items.length - idx }),
                })
                  .then((r) => r.json())
                  .then((j) => {
                    delNext(idx - 1);
                  });
              };
              delNext(-diff);
            }
          };
          // Insert
          document.getElementById("fet-btn-add-day").onclick = function () {
            // FET algoritması: D{n+1}, Day {n+1}
            let newName = `D${items.length + 1}`;
            let newLongName = `Day ${items.length + 1}`;
            Swal.fire({
              title: "Yeni Gün",
              html: `<input id="fet-new-name-day" class="swal2-input" value="${newName}" placeholder="Kısa Ad"><input id="fet-new-longname-day" class="swal2-input" value="${newLongName}" placeholder="Uzun Ad">`,
              focusConfirm: false,
              preConfirm: () => {
                return {
                  name: document.getElementById("fet-new-name-day").value,
                  longName: document.getElementById("fet-new-longname-day")
                    .value,
                };
              },
            }).then((res) => {
              if (res.isConfirmed && res.value.name) {
                fetch("/Home/AddDay", {
                  method: "POST",
                  headers: { "Content-Type": "application/json" },
                  body: JSON.stringify({
                    Name: res.value.name,
                    LongName: res.value.longName,
                  }),
                })
                  .then((r) => r.json())
                  .then((j) => {
                    if (j.success) window.fetShowDays();
                    else
                      Swal.fire(
                        "Hata",
                        j.message || "Ekleme başarısız",
                        "error"
                      );
                  });
              }
            });
          };
          // Edit
          document.getElementById("fet-btn-edit-day").onclick = function () {
            if (list && list.querySelector(".active")) {
              let idx = parseInt(
                list.querySelector(".active").getAttribute("data-idx")
              );
              let item = items[idx];
              Swal.fire({
                title: "Gün Düzenle",
                html: `<input id="fet-edit-name-day" class="swal2-input" value="${
                  item.name || ""
                }" placeholder="Kısa Ad"><input id="fet-edit-longname-day" class="swal2-input" value="${
                  item.longName || item.name || ""
                }" placeholder="Uzun Ad">`,
                focusConfirm: false,
                preConfirm: () => {
                  return {
                    name: document.getElementById("fet-edit-name-day").value,
                    longName: document.getElementById("fet-edit-longname-day")
                      .value,
                  };
                },
              }).then((res) => {
                if (res.isConfirmed && res.value.name) {
                  fetch("/Home/EditDay", {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({
                      Index: idx,
                      Name: res.value.name,
                      LongName: res.value.longName,
                    }),
                  })
                    .then((r) => r.json())
                    .then((j) => {
                      if (j.success) window.fetShowDays();
                      else
                        Swal.fire(
                          "Hata",
                          j.message || "Düzenleme başarısız",
                          "error"
                        );
                    });
                }
              });
            } else {
              Swal.fire("Uyarı", "Lütfen bir satır seçin", "warning");
            }
          };
          // Delete
          document.getElementById("fet-btn-del-day").onclick = function () {
            if (list && list.querySelector(".active")) {
              let idx = parseInt(
                list.querySelector(".active").getAttribute("data-idx")
              );
              Swal.fire({
                title: "Silmek istediğinize emin misiniz?",
                icon: "warning",
                showCancelButton: true,
                confirmButtonText: "Evet, Sil",
                cancelButtonText: "İptal",
              }).then((res) => {
                if (res.isConfirmed) {
                  fetch("/Home/DeleteDay", {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({ Index: idx }),
                  })
                    .then((r) => r.json())
                    .then((j) => {
                      if (j.success) window.fetShowDays();
                      else
                        Swal.fire(
                          "Hata",
                          j.message || "Silme başarısız",
                          "error"
                        );
                    });
                }
              });
            } else {
              Swal.fire("Uyarı", "Lütfen bir satır seçin", "warning");
            }
          };
        },
      });
    });
};

window.fetShowHours = function () {
  if (!window.fetCurrentFileName) {
    Swal.fire("Hata", "Önce bir proje açın veya oluşturun", "error");
    return;
  }
  fetch("/Home/ShowData?dataType=hours")
    .then((r) => r.json())
    .then((data) => {
      let items = data.success && data.data ? data.data : [];
      let html = `
<div class="mb-2 d-flex align-items-center">
    <label class="me-2">Hours:</label>
    <input type="number" class="form-control form-control-sm" style="width:90px;" min="1" max="14" value="${
      items.length
    }" id="fet-spin-main-hours" />
</div>
<ul class="list-group mb-2" id="fet-list-main-hours" style="height:220px;overflow:auto;">
    ${items
      .map(
        (item, i) =>
          `<li class="list-group-item fet-selectable" data-idx="${i}">${
            i + 1
          }. N: ${item.name || ""}, LN: ${
            item.longName || item.name || ""
          }</li>`
      )
      .join("")}
</ul>
<div class="d-flex gap-2">
    <button class="btn btn-outline-success btn-sm w-100" id="fet-btn-add-hour">Insert</button>
    <button class="btn btn-outline-warning btn-sm w-100" id="fet-btn-edit-hour">Düzenle</button>
    <button class="btn btn-outline-danger btn-sm w-100" id="fet-btn-del-hour">Kaldır</button>
</div>`;
      Swal.fire({
        title: "Saatler",
        html: html,
        width: "40%",
        heightAuto: false,
        showConfirmButton: false,
        customClass: {
          popup: "swal-wide",
          htmlContainer: "swal-scrollable",
          content: "text-start",
        },
        didOpen: () => {
          let selectedIdx = 0;
          const list = document.getElementById("fet-list-main-hours");
          if (list) {
            const itemsEls = list.querySelectorAll(".fet-selectable");
            if (itemsEls.length > 0) itemsEls[0].classList.add("active");
            itemsEls.forEach((el) => {
              el.onclick = function () {
                itemsEls.forEach((e) => e.classList.remove("active"));
                this.classList.add("active");
                selectedIdx = parseInt(this.getAttribute("data-idx"));
              };
            });
          }
          // Number input değişimi
          document.getElementById("fet-spin-main-hours").onchange =
            function () {
              let newCount = parseInt(this.value);
              let diff = newCount - items.length;
              if (diff > 0) {
                // Ekle
                let lastNum = items.length;
                let addNext = (idx) => {
                  if (idx <= 0) {
                    window.fetShowHours();
                    return;
                  }
                  let newName = `H${lastNum + 1}`;
                  let newLongName = `Hour ${lastNum + 1}`;
                  fetch("/Home/AddHour", {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({
                      Name: newName,
                      LongName: newLongName,
                    }),
                  })
                    .then((r) => r.json())
                    .then((j) => {
                      lastNum++;
                      addNext(idx - 1);
                    });
                };
                addNext(diff);
              } else if (diff < 0) {
                // Sil
                let delNext = (idx) => {
                  if (idx <= 0) {
                    window.fetShowHours();
                    return;
                  }
                  fetch("/Home/DeleteHour", {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({ Index: items.length - idx }),
                  })
                    .then((r) => r.json())
                    .then((j) => {
                      delNext(idx - 1);
                    });
                };
                delNext(-diff);
              }
            };
          // Insert
          document.getElementById("fet-btn-add-hour").onclick = function () {
            let newName = `H${items.length + 1}`;
            let newLongName = `Hour ${items.length + 1}`;
            Swal.fire({
              title: "Yeni Saat",
              html: `<input id="fet-new-name-hour" class="swal2-input" value="${newName}" placeholder="Kısa Ad"><input id="fet-new-longname-hour" class="swal2-input" value="${newLongName}" placeholder="Uzun Ad">`,
              focusConfirm: false,
              preConfirm: () => {
                return {
                  name: document.getElementById("fet-new-name-hour").value,
                  longName: document.getElementById("fet-new-longname-hour")
                    .value,
                };
              },
            }).then((res) => {
              if (res.isConfirmed && res.value.name) {
                fetch("/Home/AddHour", {
                  method: "POST",
                  headers: { "Content-Type": "application/json" },
                  body: JSON.stringify({
                    Name: res.value.name,
                    LongName: res.value.longName,
                  }),
                })
                  .then((r) => r.json())
                  .then((j) => {
                    if (j.success) window.fetShowHours();
                    else
                      Swal.fire(
                        "Hata",
                        j.message || "Ekleme başarısız",
                        "error"
                      );
                  });
              }
            });
          };
          // Edit
          document.getElementById("fet-btn-edit-hour").onclick = function () {
            if (list && list.querySelector(".active")) {
              let idx = parseInt(
                list.querySelector(".active").getAttribute("data-idx")
              );
              let item = items[idx];
              Swal.fire({
                title: "Saat Düzenle",
                html: `<input id="fet-edit-name-hour" class="swal2-input" value="${
                  item.name || ""
                }" placeholder="Kısa Ad"><input id="fet-edit-longname-hour" class="swal2-input" value="${
                  item.longName || item.name || ""
                }" placeholder="Uzun Ad">`,
                focusConfirm: false,
                preConfirm: () => {
                  return {
                    name: document.getElementById("fet-edit-name-hour").value,
                    longName: document.getElementById("fet-edit-longname-hour")
                      .value,
                  };
                },
              }).then((res) => {
                if (res.isConfirmed && res.value.name) {
                  fetch("/Home/EditHour", {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({
                      Index: idx,
                      Name: res.value.name,
                      LongName: res.value.longName,
                    }),
                  })
                    .then((r) => r.json())
                    .then((j) => {
                      if (j.success) window.fetShowHours();
                      else
                        Swal.fire(
                          "Hata",
                          j.message || "Düzenleme başarısız",
                          "error"
                        );
                    });
                }
              });
            } else {
              Swal.fire("Uyarı", "Lütfen bir satır seçin", "warning");
            }
          };
          // Delete
          document.getElementById("fet-btn-del-hour").onclick = function () {
            if (list && list.querySelector(".active")) {
              let idx = parseInt(
                list.querySelector(".active").getAttribute("data-idx")
              );
              Swal.fire({
                title: "Silmek istediğinize emin misiniz?",
                icon: "warning",
                showCancelButton: true,
                confirmButtonText: "Evet, Sil",
                cancelButtonText: "İptal",
              }).then((res) => {
                if (res.isConfirmed) {
                  fetch("/Home/DeleteHour", {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({ Index: idx }),
                  })
                    .then((r) => r.json())
                    .then((j) => {
                      if (j.success) window.fetShowHours();
                      else
                        Swal.fire(
                          "Hata",
                          j.message || "Silme başarısız",
                          "error"
                        );
                    });
                }
              });
            } else {
              Swal.fire("Uyarı", "Lütfen bir satır seçin", "warning");
            }
          };
        },
      });
    });
};

// Constraint dialog function
window.openConstraintDialog = function (constraintType) {
  // Önce mevcut kısıtları getir ve listele
  fetch("/Home/ListConstraints?type=" + constraintType)
    .then((r) => r.json())
    .then((data) => {
      let listHtml = `<select id="constraint-list" size="15" class="form-select" style="width:45%;height:350px;">${data.list
        .map((c, i) => `<option value="${i}">${c.summary}</option>`)
        .join("")}</select>`;
      let detailHtml = `<div id="constraint-detail" style="width:55%;height:350px;overflow:auto;border:1px solid #eee;padding:8px;">${
        data.list.length ? data.list[0].details : ""
      }</div>`;
      Swal.fire({
        title: "Kısıt Listesi",
        html: `<div class="d-flex gap-2">${listHtml}${detailHtml}</div>
            <div class="mt-2 d-flex gap-2">
              <button id="btn-add" class="btn btn-success">Ekle</button>
              <button id="btn-edit" class="btn btn-warning">Düzenle</button>
              <button id="btn-del" class="btn btn-danger">Kaldır</button>
            </div>`,
        showConfirmButton: false,
        width: "900px",
        didOpen: () => {
          document.getElementById("btn-add").onclick = () =>
            window.openConstraintAddDialog(constraintType);
          document.getElementById("btn-edit").onclick = () => {
            const idx = document.getElementById("constraint-list").value;
            window.openConstraintEditForm(constraintType, idx, data.list[idx]);
          };
          document.getElementById("btn-del").onclick = () => {
            const idx = document.getElementById("constraint-list").value;
            window.deleteConstraint(constraintType, idx, data.list[idx]);
          };
          document.getElementById("constraint-list").onchange = (e) => {
            let idx = e.target.value;
            document.getElementById("constraint-detail").innerHTML =
              data.list[idx].details;
          };
        },
      });
    });
};

// Ekleme ekranı (mevcut ekleme fonksiyonunu çağır)
window.openConstraintAddDialog = function (constraintType) {
  Swal.close();
  // 1. ConstraintActivityPreferredTimeSlots
  if (constraintType === "ConstraintActivityPreferredTimeSlots") {
    fetch("/Home/GetDaysAndHours")
      .then((r) => r.json())
      .then((data) => {
        fetch("/Home/ShowData?dataType=activities")
          .then((r2) => r2.json())
          .then((actData) => {
            if (!data.success) {
              Swal.fire(
                "Hata",
                data.message || "Gün ve saatler alınamadı",
                "error"
              );
              return;
            }
            if (!actData.success) {
              Swal.fire(
                "Hata",
                actData.message || "Aktiviteler alınamadı",
                "error"
              );
              return;
            }
            const days = data.days.filter(Boolean);
            const hours = data.hours.filter(Boolean);
            const activities = actData.data || [];
            let forbiddenSlots = new Set();
            let activityOptions = activities
              .map((a) => {
                let subject =
                  a.subject !== undefined &&
                  a.subject !== null &&
                  a.subject !== ""
                    ? a.subject
                    : "Bilinmiyor";
                let id = a.id !== undefined && a.id !== null ? a.id : "-";
                return `<option value="${id}">${id} - ${subject}</option>`;
              })
              .join("");
            let activityHtml = `<div class='mb-2'><label>Etkinlik:</label><select id='activity-select' class='form-select'>${activityOptions}</select></div>`;
            let tableHtml =
              '<table class="table table-bordered text-center align-middle"><thead><tr><th></th>';
            days.forEach((day) => {
              tableHtml += `<th>${day}</th>`;
            });
            tableHtml += "</tr></thead><tbody>";
            hours.forEach((hour, hIdx) => {
              tableHtml += `<tr><th>${hour}</th>`;
              days.forEach((day, dIdx) => {
                const cellId = `${day}-${hour}`;
                // Başlangıçta kırmızı (yasaklı), tıklayınca yeşil (izinli)
                tableHtml += `<td id="cell-${cellId}" style="background:#e53935;cursor:pointer;" onclick="toggleSlot('${cellId}')"></td>`;
              });
              tableHtml += "</tr>";
            });
            tableHtml += "</tbody></table>";
            window.toggleSlot = function (cellId) {
              const td = document.getElementById("cell-" + cellId);
              if (!td) return;
              if (forbiddenSlots.has(cellId)) {
                forbiddenSlots.delete(cellId);
                td.style.background = "#e53935"; // Kırmızı (yasaklı)
              } else {
                forbiddenSlots.add(cellId);
                td.style.background = "#4caf50"; // Yeşil (izinli)
              }
            };
            Swal.fire({
              title: "Etkinlik Tercihli Zaman Slotları",
              html: `
            ${activityHtml}
            <div class='mb-2'>Yeşil: İzinli, Kırmızı: Yasaklı</div>
            ${tableHtml}
            <div class="mb-3">
                <label class="form-label">Açıklama</label>
                <textarea class="form-control" id="constraint-comments" rows="2"></textarea>
            </div>
          `,
              width: "800px",
              showCancelButton: true,
              confirmButtonText: "Kaydet",
              cancelButtonText: "İptal",
              preConfirm: () => {
                let selectedActivityId = parseInt(
                  document.getElementById("activity-select").value
                );
                let result = [];
                forbiddenSlots.forEach((cellId) => {
                  const [day, hour] = cellId.split("-");
                  result.push({ Day: day, Hour: hour });
                });
                let dataObj = {
                  WeightPercentage: 100,
                  ActivityId: selectedActivityId,
                  PreferredTimeSlots: result,
                  NumberOfPreferredTimeSlots: result.length,
                  Active: true,
                  Comments:
                    document.getElementById("constraint-comments").value || "",
                };
                return {
                  Type: constraintType,
                  Data: dataObj,
                };
              },
            }).then((result) => {
              if (result.isConfirmed && result.value) {
                fetch("/Home/AddConstraint", {
                  method: "POST",
                  headers: { "Content-Type": "application/json" },
                  body: JSON.stringify(result.value),
                })
                  .then((response) => response.json())
                  .then((data) => {
                    if (data.success) {
                      Swal.fire(
                        "Başarılı",
                        "Kısıt başarıyla kaydedildi",
                        "success"
                      );
                      if (data.xmlContent) {
                        window.fetCurrentXmlContent = data.xmlContent;
                      }
                    } else {
                      Swal.fire(
                        "Hata",
                        data.message || "Kısıt kaydedilemedi",
                        "error"
                      );
                    }
                  });
              }
            });
          });
      });
    return;
  }
  // ConstraintBreakTimes: Kapatılacak Zaman Bloğu (period/slot table selection)
  if (constraintType === "ConstraintBreakTimes") {
    fetch("/Home/GetDaysAndHours")
      .then((r) => r.json())
      .then((data) => {
        if (!data.success) {
          Swal.fire(
            "Hata",
            data.message || "Gün ve saatler alınamadı",
            "error"
          );
          return;
        }
        const days = data.days.filter(Boolean);
        const hours = data.hours.filter(Boolean);
        let breakSlots = new Set();
        let tableHtml =
          '<table class="table table-bordered text-center align-middle"><thead><tr><th></th>';
        days.forEach((day) => {
          tableHtml += `<th>${day}</th>`;
        });
        tableHtml += "</tr></thead><tbody>";
        hours.forEach((hour, hIdx) => {
          tableHtml += `<tr><th>${hour}</th>`;
          days.forEach((day, dIdx) => {
            const cellId = `${dIdx}_${hIdx}`;
            tableHtml += `<td id='break-cell-${cellId}' onclick='window.toggleBreakSlot("${cellId}")' style='cursor:pointer;background:#4caf50;'><span style='display:inline-block;width:18px;height:18px;'></span></td>`;
          });
          tableHtml += "</tr>";
        });
        tableHtml += "</tbody></table>";
        window.toggleBreakSlot = function (cellId) {
          const td = document.getElementById("break-cell-" + cellId);
          if (!td) return;
          if (breakSlots.has(cellId)) {
            breakSlots.delete(cellId);
            td.style.background = "#4caf50";
          } else {
            breakSlots.add(cellId);
            td.style.background = "#e74c3c";
          }
        };
        Swal.fire({
          title: "Kapatılacak Zaman Bloğu",
          html: `
          <div class='mb-2'>Kırmızı: Kapalı, Yeşil: Açık</div>
          ${tableHtml}
          <div class="mb-3">
              <label class="form-label">Açıklama</label>
              <textarea class="form-control" id="constraint-comments" rows="2"></textarea>
          </div>
        `,
          width: "800px",
          showCancelButton: true,
          confirmButtonText: "Kaydet",
          cancelButtonText: "İptal",
          preConfirm: () => {
            let result = [];
            breakSlots.forEach((cellId) => {
              const [dIdx, hIdx] = cellId.split("_").map(Number);
              result.push({ Day: days[dIdx], Hour: hours[hIdx] });
            });
            let dataObj = {
              WeightPercentage: 100,
              BreakTimes: result,
              NumberOfBreakTimes: result.length,
              Active: true,
              Comments:
                document.getElementById("constraint-comments").value || "",
            };
            return {
              Type: constraintType,
              Data: dataObj,
            };
          },
        }).then((result) => {
          if (result.isConfirmed && result.value) {
            fetch("/Home/AddConstraint", {
              method: "POST",
              headers: { "Content-Type": "application/json" },
              body: JSON.stringify(result.value),
            })
              .then((response) => response.json())
              .then((data) => {
                if (data.success) {
                  Swal.fire("Başarılı", "Kısıt eklendi", "success");
                } else {
                  Swal.fire(
                    "Hata",
                    data.message || "Kısıt eklenemedi",
                    "error"
                  );
                }
              });
          }
        });
      });
    return;
  }
  if (
    constraintType === "ConstraintStudentsMaxHoursDaily" ||
    constraintType === "ConstraintStudentsMaxHoursContinuously"
  ) {
    const isDaily = constraintType === "ConstraintStudentsMaxHoursDaily";
    Swal.fire({
      title: isDaily
        ? "Öğrenciler Günlük Maksimum Saat"
        : "Öğrenciler Aralıksız Maksimum Saat",
      html: `
        <div class="mb-3">
            <label class="form-label">${
              isDaily
                ? "Günlük Maksimum Ders Saati"
                : "Aralıksız Maksimum Ders Saati"
            }</label>
            <input type="number" class="form-control" id="max-hours" min="1" max="20" value="6">
        </div>
        <div class="mb-3">
            <label class="form-label">Yumuşaklık Yüzdesi (%)</label>
            <input type="number" class="form-control" id="softness" min="1" max="100" value="100">
        </div>
        <div class="mb-3">
            <label class="form-label">Açıklama</label>
            <textarea class="form-control" id="constraint-comments" rows="2"></textarea>
        </div>
      `,
      width: "400px",
      showCancelButton: true,
      confirmButtonText: "Kaydet",
      cancelButtonText: "İptal",
      preConfirm: () => {
        const maxHours = parseInt(document.getElementById("max-hours").value);
        const softness = parseInt(document.getElementById("softness").value);
        const comments =
          document.getElementById("constraint-comments").value || "";
        if (!maxHours || maxHours < 1) {
          Swal.showValidationMessage("Geçerli bir maksimum saat girin");
          return false;
        }
        if (!softness || softness < 1 || softness > 100) {
          Swal.showValidationMessage("Yumuşaklık yüzdesi 1-100 arası olmalı");
          return false;
        }
        let dataObj = isDaily
          ? {
              WeightPercentage: softness,
              MaximumHoursDaily: maxHours,
              Active: true,
              Comments: comments,
            }
          : {
              WeightPercentage: softness,
              MaximumHoursContinuously: maxHours,
              Active: true,
              Comments: comments,
            };
        return {
          Type: constraintType,
          Data: dataObj,
        };
      },
    }).then((result) => {
      if (result.isConfirmed && result.value) {
        fetch("/Home/AddConstraint", {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify(result.value),
        })
          .then((response) => response.json())
          .then((data) => {
            if (data.success) {
              Swal.fire("Başarılı", "Kısıt başarıyla kaydedildi", "success");
              if (data.xmlContent) {
                window.fetCurrentXmlContent = data.xmlContent;
              }
            } else {
              Swal.fire("Hata", data.message || "Kısıt kaydedilemedi", "error");
            }
          });
      }
    });
    return;
  }
  if (constraintType === "ConstraintMinDaysBetweenActivities") {
    fetch("/Home/ShowData?dataType=activities")
      .then((r) => r.json())
      .then((actData) => {
        let xml = window.fetCurrentXmlContent;
        if (!actData.success) {
          Swal.fire(
            "Hata",
            actData.message || "Aktiviteler alınamadı",
            "error"
          );
          return;
        }
        const activities = actData.data || [];
        let activityList = activities.map((a) => {
          let subject =
            a.subject !== undefined && a.subject !== null && a.subject !== ""
              ? a.subject
              : "Bilinmiyor";
          let id = a.id !== undefined && a.id !== null ? a.id : "-";
          return `${id} - ${subject}`;
        });
        // Seçili id'leri parse et (yeni eklemede boş olmalı)
        let selectedIds = [];
        let minDays = 1;
        let consecutive = false;
        let weight = 100;
        let comments = "";
        let html = `
    <div class="d-flex gap-2 mb-2" style="height:220px;">
        <select id="fet-min-days-activity-list" multiple size="12" class="form-select" style="width:48%;font-size:13px;">
            ${activityList.map((txt, i) => `<option value="${activities[i].id}">${txt}</option>`).join("")}
        </select>
        <select id="fet-min-days-activity-selected" multiple size="12" class="form-select" style="width:48%;font-size:13px;"></select>
    </div>
    <div class="d-flex gap-2 mb-2">
        <button type="button" class="btn btn-outline-secondary btn-sm w-100" id="fet-min-days-btn-all">Tümü</button>
        <button type="button" class="btn btn-outline-secondary btn-sm w-100" id="fet-min-days-btn-clear">Temizle</button>
    </div>
    <div class="d-flex gap-2 mb-2">
        <button type="button" class="btn btn-outline-secondary btn-sm w-100" id="fet-min-days-btn-right">&gt;</button>
        <button type="button" class="btn btn-outline-secondary btn-sm w-100" id="fet-min-days-btn-left">&lt;</button>
    </div>
    <div class="mb-2">
        <label>Min days</label>
        <input type="number" class="form-control" id="fet-min-days-value" min="1" max="30" value="${minDays}">
    </div>
    <div class="form-check mb-2">
        <input class="form-check-input" type="checkbox" value="1" id="fet-min-days-consecutive" ${consecutive ? 'checked' : ''}>
        <label class="form-check-label" for="fet-min-days-consecutive">
            Aynı günse ardışık olsun
        </label>
    </div>
    <div class="mb-2">
        <label>Yumuşaklık Yüzdesi (%)</label>
        <input type="number" class="form-control" id="fet-min-days-weight" min="0" max="100" value="${weight}">
    </div>
    <div class="mb-3">
        <label class="form-label">Açıklama</label>
        <textarea class="form-control" id="constraint-comments" rows="2">${comments}</textarea>
    </div>
    `;
        Swal.fire({
          title: "Etkinlikler Arası Minimum Gün Ekle",
          html: html,
          width: "900px",
          showCancelButton: true,
          confirmButtonText: "Kaydet",
          cancelButtonText: "İptal",
          didOpen: () => {
            const list = document.getElementById("fet-min-days-activity-list");
            const sel = document.getElementById("fet-min-days-activity-selected");
            document.getElementById("fet-min-days-btn-all").onclick = function () { sel.innerHTML = list.innerHTML; };
            document.getElementById("fet-min-days-btn-clear").onclick = function () { sel.innerHTML = ""; };
            document.getElementById("fet-min-days-btn-right").onclick = function () {
              Array.from(list.selectedOptions).forEach((opt) => {
                if (![...sel.options].some((o) => o.value === opt.value)) {
                  let newOpt = opt.cloneNode(true);
                  sel.appendChild(newOpt);
                }
              });
            };
            document.getElementById("fet-min-days-btn-left").onclick = function () {
              Array.from(sel.selectedOptions).forEach((opt) => sel.removeChild(opt));
            };
          },
          preConfirm: () => {
            const sel = document.getElementById("fet-min-days-activity-selected");
            const selectedIds = Array.from(sel.options).map((o) => parseInt(o.value)).filter(Boolean);
            const minDays = parseInt(document.getElementById("fet-min-days-value").value);
            const consecutive = document.getElementById("fet-min-days-consecutive").checked;
            const weight = parseFloat(document.getElementById("fet-min-days-weight").value);
            const comments = document.getElementById("constraint-comments").value || '';
            if (!selectedIds.length) {
              Swal.showValidationMessage("En az bir etkinlik seçmelisiniz");
              return false;
            }
            if (!minDays || minDays < 1) {
              Swal.showValidationMessage("Geçerli bir minimum gün girin");
              return false;
            }
            if (isNaN(weight) || weight < 0 || weight > 100) {
              Swal.showValidationMessage("Ağırlık yüzdesi 0-100 arası olmalı");
              return false;
            }
            let dataObj = {
              WeightPercentage: weight,
              ActivityIds: selectedIds,
              MinDays: minDays,
              ConsecutiveIfSameDay: consecutive,
              Active: true,
              Comments: comments
            };
            return {
              Type: constraintType,
              Data: dataObj
            };
          },
        }).then((result) => {
          if (result.isConfirmed && result.value) {
            fetch("/Home/AddConstraint", {
              method: "POST",
              headers: { "Content-Type": "application/json" },
              body: JSON.stringify(result.value),
            })
              .then((response) => response.json())
              .then((data) => {
                if (data.success) {
                  Swal.fire("Başarılı", "Kısıt başarıyla kaydedildi", "success");
                  if (data.xmlContent) {
                    window.fetCurrentXmlContent = data.xmlContent;
                  }
                } else {
                  Swal.fire("Hata", data.message || "Kısıt kaydedilemedi", "error");
                }
              });
          }
        });
      });
    return;
  }

  if (constraintType === "ConstraintActivitiesNotOverlapping") {
    fetch("/Home/ShowData?dataType=activities")
      .then((r) => r.json())
      .then((actData) => {
        let xml = window.fetCurrentXmlContent;
        if (!actData.success) {
          Swal.fire(
            "Hata",
            actData.message || "Aktiviteler alınamadı",
            "error"
          );
          return;
        }
        const activities = actData.data || [];
        let activityList = activities.map((a) => {
          let subject = a.subject !== undefined && a.subject !== null && a.subject !== "" ? a.subject : "Bilinmiyor";
          let id = a.id !== undefined && a.id !== null ? a.id : "-";
          return `${id} - ${subject}`;
        });
        // Seçili id'leri parse et (yeni eklemede boş olmalı)
        let selectedIds = [];
        let comments = "";
        let html = `
    <div class="d-flex gap-2 mb-2" style="height:220px;">
        <select id="fet-notoverlap-activity-list" multiple size="12" class="form-select" style="width:48%;font-size:13px;">
            ${activityList.map((txt, i) => `<option value="${activities[i].id}">${txt}</option>`).join("")}
        </select>
        <select id="fet-notoverlap-activity-selected" multiple size="12" class="form-select" style="width:48%;font-size:13px;"></select>
    </div>
    <div class="d-flex gap-2 mb-2">
        <button type="button" class="btn btn-outline-secondary btn-sm w-100" id="fet-notoverlap-btn-all">Tümü</button>
        <button type="button" class="btn btn-outline-secondary btn-sm w-100" id="fet-notoverlap-btn-clear">Temizle</button>
    </div>
    <div class="d-flex gap-2 mb-2">
        <button type="button" class="btn btn-outline-secondary btn-sm w-100" id="fet-notoverlap-btn-right">&gt;</button>
        <button type="button" class="btn btn-outline-secondary btn-sm w-100" id="fet-notoverlap-btn-left">&lt;</button>
    </div>
    <div class="mb-3">
        <label class="form-label">Açıklama</label>
        <textarea class="form-control" id="constraint-comments" rows="2">${comments}</textarea>
    </div>
    `;
        Swal.fire({
          title: "Etkinlikler Çakışmasın Ekle",
          html: html,
          width: "900px",
          showCancelButton: true,
          confirmButtonText: "Kaydet",
          cancelButtonText: "İptal",
          didOpen: () => {
            const list = document.getElementById("fet-notoverlap-activity-list");
            const sel = document.getElementById("fet-notoverlap-activity-selected");
            document.getElementById("fet-notoverlap-btn-all").onclick = function () { sel.innerHTML = list.innerHTML; };
            document.getElementById("fet-notoverlap-btn-clear").onclick = function () { sel.innerHTML = ""; };
            document.getElementById("fet-notoverlap-btn-right").onclick = function () {
              Array.from(list.selectedOptions).forEach((opt) => {
                if (![...sel.options].some((o) => o.value === opt.value)) {
                  let newOpt = opt.cloneNode(true);
                  sel.appendChild(newOpt);
                }
              });
            };
            document.getElementById("fet-notoverlap-btn-left").onclick = function () {
              Array.from(sel.selectedOptions).forEach((opt) => sel.removeChild(opt));
            };
          },
          preConfirm: () => {
            const sel = document.getElementById("fet-notoverlap-activity-selected");
            const selectedIds = Array.from(sel.options).map((o) => parseInt(o.value)).filter(Boolean);
            const comments = document.getElementById("constraint-comments").value || '';
            if (!selectedIds.length) {
              Swal.showValidationMessage("En az bir etkinlik seçmelisiniz");
              return false;
            }
            let dataObj = {
              WeightPercentage: 100,
              ActivityIds: selectedIds,
              Active: true,
              Comments: comments
            };
            return {
              Type: constraintType,
              Data: dataObj
            };
          },
        }).then((result) => {
          if (result.isConfirmed && result.value) {
            fetch("/Home/AddConstraint", {
              method: "POST",
              headers: { "Content-Type": "application/json" },
              body: JSON.stringify(result.value),
            })
              .then((response) => response.json())
              .then((data) => {
                if (data.success) {
                  Swal.fire("Başarılı", "Kısıt başarıyla kaydedildi", "success");
                  if (data.xmlContent) {
                    window.fetCurrentXmlContent = data.xmlContent;
                  }
                } else {
                  Swal.fire("Hata", data.message || "Kısıt kaydedilemedi", "error");
                }
              });
          }
        });
      });
    return;
  }
  // Generic constraint ekleme (ör: ConstraintBreakTimes, ConstraintBasicCompulsoryTime)
  setTimeout(function () {
    const constraintConfig = {
      ConstraintBreakTimes: {
        title: "Kapatılacak Zaman Bloğu",
        fields: [
          {
            name: "Break_Times",
            label: "Kapatılacak Zaman Bloğu",
            type: "text",
            placeholder: "Pazartesi-3,Salı-5",
          },
        ],
      },
      ConstraintBasicCompulsoryTime: {
        title: "Temel Zorunlu Zaman Kısıtı",
        fields: [],
      },
    };
    const config = constraintConfig[constraintType];
    if (!config) {
      Swal.fire("Hata", "Geçersiz kısıt türü", "error");
      return;
    }
    let formHtml = `
      <div class="mb-3">
          <label class="form-label">Ağırlık (%)</label>
          <input type="number" class="form-control" id="constraint-weight" value="100" min="1" max="100">
      </div>
      <div class="mb-3">
          <label class="form-label">Aktif</label>
          <select class="form-select" id="constraint-active">
              <option value="true">Evet</option>
              <option value="false">Hayır</option>
          </select>
      </div>
    `;
    config.fields.forEach((field) => {
      formHtml += `
        <div class="mb-3">
            <label class="form-label">${field.label}</label>
            <input type="${field.type}" class="form-control" id="constraint-${
        field.name
      }" placeholder="${field.placeholder || ""}">
        </div>
      `;
    });
    formHtml += `
      <div class="mb-3">
          <label class="form-label">Açıklama</label>
          <textarea class="form-control" id="constraint-comments" rows="2"></textarea>
      </div>
    `;
    Swal.fire({
      title: config.title,
      html: formHtml,
      width: "600px",
      showCancelButton: true,
      confirmButtonText: "Kaydet",
      cancelButtonText: "İptal",
      preConfirm: () => {
        const dataObj = {
          WeightPercentage:
            parseFloat(document.getElementById("constraint-weight").value) ||
            100,
          Active: document.getElementById("constraint-active").value === "true",
          Comments: document.getElementById("constraint-comments").value || "",
        };
        config.fields.forEach((field) => {
          const element = document.getElementById(`constraint-${field.name}`);
          if (element) {
            if (field.type === "number") {
              dataObj[field.name] = parseInt(element.value) || 0;
            } else if (field.name === "Activity_Id") {
              dataObj[field.name] = element.value
                .split(",")
                .map((x) => parseInt(x.trim()))
                .filter((x) => !isNaN(x));
            } else if (field.name === "Break_Times") {
              dataObj[field.name] = element.value
                .split(",")
                .map((x) => {
                  const [day, hour] = x.split("-");
                  return { Day: day.trim(), Hour: parseInt(hour) };
                })
                .filter((x) => x.Day && !isNaN(x.Hour));
            } else {
              dataObj[field.name] = element.value;
            }
          }
        });
        return { Type: constraintType, Data: dataObj };
      },
    }).then((result) => {
      if (result.isConfirmed && result.value) {
        fetch("/Home/AddConstraint", {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify(result.value),
        })
          .then((response) => response.json())
          .then((data) => {
            if (data.success) {
              Swal.fire("Başarılı", "Kısıt başarıyla kaydedildi", "success");
              if (data.xmlContent) {
                window.fetCurrentXmlContent = data.xmlContent;
              }
            } else {
              Swal.fire("Hata", data.message || "Kısıt kaydedilemedi", "error");
            }
          });
      }
    });
  }, 50);
};

window.openConstraintEditForm = function (constraintType, idx, constraint) {
  fetch("/Home/ListConstraints?type=" + constraintType)
    .then((r) => r.json())
    .then((data) => {
      const cIdx = parseInt(idx);
      if (!data.list[cIdx]) {
        Swal.fire("Hata", "Düzenlenecek kısıt bulunamadı", "error");
        return;
      }
      let xml = data.list[cIdx].details;
      let getVal = (tag) => {
        let m = xml.match(new RegExp(`<${tag}>(.*?)</${tag}>`, "i"));
        return m ? m[1] : "";
      };
      // --- DÜZENLEME MODALINI KISIT TÜRÜNE GÖRE OLUŞTUR ---
      if (constraintType === "ConstraintMinDaysBetweenActivities" || constraintType === "ConstraintActivitiesNotOverlapping") {
        fetch("/Home/ShowData?dataType=activities")
          .then((r) => r.json())
          .then((actData) => {
            const activities = actData.data || [];
            let activityList = activities.map((a) => {
              let subject =
                a.subject !== undefined &&
                a.subject !== null &&
                a.subject !== ""
                  ? a.subject
                  : "Bilinmiyor";
              let id = a.id !== undefined && a.id !== null ? a.id : "-";
              return `${id} - ${subject}`;
            });
            // Seçili id'leri parse et
            let selectedIds = [];
            let re = /<Activity_Id>(.*?)<\/Activity_Id>/g;
            let m;
            while ((m = re.exec(xml))) selectedIds.push(parseInt(m[1]));
            let minDays = parseInt(getVal("MinDays")) || 1;
            let consecutive = getVal("Consecutive_If_Same_Day") === "true";
            let weight = parseFloat(getVal("Weight_Percentage")) || 100;
            let comments = getVal("Comments") || "";
            let html = `
    <div class="d-flex gap-2 mb-2" style="height:220px;">
        <select id="fet-min-days-activity-list" multiple size="12" class="form-select" style="width:48%;font-size:13px;">
            ${activityList.map((txt, i) => `<option value="${activities[i].id}" ${selectedIds.includes(activities[i].id) ? 'selected' : ''}>${txt}</option>`).join("")}
        </select>
        <select id="fet-min-days-activity-selected" multiple size="12" class="form-select" style="width:48%;font-size:13px;">${activityList.map((txt, i) => selectedIds.includes(activities[i].id) ? `<option value="${activities[i].id}">${txt}</option>` : '').join("")}</select>
    </div>
    <div class="d-flex gap-2 mb-2">
        <button type="button" class="btn btn-outline-secondary btn-sm w-100" id="fet-min-days-btn-all">Tümü</button>
        <button type="button" class="btn btn-outline-secondary btn-sm w-100" id="fet-min-days-btn-clear">Temizle</button>
    </div>
    <div class="d-flex gap-2 mb-2">
        <button type="button" class="btn btn-outline-secondary btn-sm w-100" id="fet-min-days-btn-right">&gt;</button>
        <button type="button" class="btn btn-outline-secondary btn-sm w-100" id="fet-min-days-btn-left">&lt;</button>
    </div>
    <div class="mb-2">
        <label>Min days</label>
        <input type="number" class="form-control" id="fet-min-days-value" min="1" max="30" value="${minDays}">
    </div>
    <div class="form-check mb-2">
        <input class="form-check-input" type="checkbox" value="1" id="fet-min-days-consecutive" ${consecutive ? 'checked' : ''}>
        <label class="form-check-label" for="fet-min-days-consecutive">
            Aynı günse ardışık olsun
        </label>
    </div>
    <div class="mb-2">
        <label>Yumuşaklık Yüzdesi (%)</label>
        <input type="number" class="form-control" id="fet-min-days-weight" min="0" max="100" value="${weight}">
    </div>
    <div class="mb-3">
        <label class="form-label">Açıklama</label>
        <textarea class="form-control" id="constraint-comments" rows="2">${comments}</textarea>
    </div>
    `;
            Swal.fire({
              title: "Etkinlikler Arası Minimum Gün Düzenle",
              html: html,
              width: "900px",
              showCancelButton: true,
              confirmButtonText: "Kaydet",
              cancelButtonText: "İptal",
              didOpen: () => {
                const list = document.getElementById("fet-min-days-activity-list");
                const sel = document.getElementById("fet-min-days-activity-selected");
                document.getElementById("fet-min-days-btn-all").onclick = function () { sel.innerHTML = list.innerHTML; };
                document.getElementById("fet-min-days-btn-clear").onclick = function () { sel.innerHTML = ""; };
                document.getElementById("fet-min-days-btn-right").onclick = function () {
                  Array.from(list.selectedOptions).forEach((opt) => {
                    if (![...sel.options].some((o) => o.value === opt.value)) {
                      let newOpt = opt.cloneNode(true);
                      sel.appendChild(newOpt);
                    }
                  });
                };
                document.getElementById("fet-min-days-btn-left").onclick = function () {
                  Array.from(sel.selectedOptions).forEach((opt) => sel.removeChild(opt));
                };
              },
              preConfirm: () => {
                const sel = document.getElementById("fet-min-days-activity-selected");
                const selectedIds = Array.from(sel.options).map((o) => parseInt(o.value)).filter(Boolean);
                const minDays = parseInt(document.getElementById("fet-min-days-value").value);
                const consecutive = document.getElementById("fet-min-days-consecutive").checked;
                const weight = parseFloat(document.getElementById("fet-min-days-weight").value);
                const comments = document.getElementById("constraint-comments").value || '';
                if (!selectedIds.length) {
                  Swal.showValidationMessage("En az bir etkinlik seçmelisiniz");
                  return false;
                }
                if (!minDays || minDays < 1) {
                  Swal.showValidationMessage("Geçerli bir minimum gün girin");
                  return false;
                }
                if (isNaN(weight) || weight < 0 || weight > 100) {
                  Swal.showValidationMessage("Ağırlık yüzdesi 0-100 arası olmalı");
                  return false;
                }
                let dataObj = {
                  WeightPercentage: weight,
                  ActivityIds: selectedIds,
                  MinDays: minDays,
                  ConsecutiveIfSameDay: consecutive,
                  Active: true,
                  Comments: comments
                };
                return {
                  Type: constraintType,
                  Data: dataObj
                };
              },
            }).then((result) => {
              if (result.isConfirmed && result.value) {
                fetch("/Home/EditConstraint", {
                  method: "POST",
                  headers: { "Content-Type": "application/json" },
                  body: JSON.stringify({
                    Type: constraintType,
                    Index: cIdx,
                    Data: result.value,
                  }),
                })
                  .then((response) => response.json())
                  .then((data) => {
                    if (data.success) {
                      Swal.fire("Başarılı", "Kısıt başarıyla güncellendi", "success");
                      setTimeout(() => openConstraintDialog(constraintType), 500);
                    } else {
                      Swal.fire("Hata", data.message || "Kısıt güncellenemedi", "error");
                    }
                  });
              }
            });
          });
        return;
      }
      // Generic constraint edit (config tabanlı)
      const config = constraintConfig[constraintType];
      if (!config) {
        Swal.fire("Hata", "Geçersiz kısıt türü", "error");
        return;
      }
      let formHtml = `
      <div class="mb-3">
          <label class="form-label">Ağırlık (%)</label>
          <input type="number" class="form-control" id="constraint-weight" value="${getVal("Weight_Percentage") || 100}" min="1" max="100">
      </div>
      <div class="mb-3">
          <label class="form-label">Aktif</label>
          <select class="form-select" id="constraint-active">
              <option value="true" ${getVal("Active") === "true" ? "selected" : ""}>Evet</option>
              <option value="false" ${getVal("Active") === "false" ? "selected" : ""}>Hayır</option>
          </select>
      </div>
    `;
      config.fields.forEach((field) => {
        formHtml += `
        <div class="mb-3">
            <label class="form-label">${field.label}</label>
            <input type="${field.type}" class="form-control" id="constraint-${field.name}" placeholder="${field.placeholder || ""}" value="${getVal(field.name) || ""}">
        </div>
      `;
      });
      formHtml += `
      <div class="mb-3">
          <label class="form-label">Açıklama</label>
          <textarea class="form-control" id="constraint-comments" rows="2">${getVal("Comments") || ""}</textarea>
      </div>
    `;
      Swal.fire({
        title: config.title + " Düzenle",
        html: formHtml,
        width: "600px",
        showCancelButton: true,
        confirmButtonText: "Kaydet",
        cancelButtonText: "İptal",
        preConfirm: () => {
          const dataObj = {
            WeightPercentage:
              parseFloat(document.getElementById("constraint-weight").value) || 100,
            Active: document.getElementById("constraint-active").value === "true",
            Comments: document.getElementById("constraint-comments").value || "",
          };
          config.fields.forEach((field) => {
            dataObj[field.name] = document.getElementById(`constraint-${field.name}`).value;
          });
          return { Type: constraintType, Index: cIdx, Data: dataObj };
        },
      }).then((result) => {
        if (result.isConfirmed && result.value) {
          fetch("/Home/EditConstraint", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(result.value),
          })
            .then((response) => response.json())
            .then((data) => {
              if (data.success) {
                Swal.fire("Başarılı", "Kısıt başarıyla güncellendi", "success");
                setTimeout(() => openConstraintDialog(constraintType), 500);
              } else {
                Swal.fire("Hata", data.message || "Kısıt güncellenemedi", "error");
              }
            });
        }
      });
    });
};

window.deleteConstraint = function (constraintType, idx, constraint) {
  fetch("/Home/ListConstraints?type=" + constraintType)
    .then((r) => r.json())
    .then((data) => {
      const cIdx = parseInt(idx);
      if (!data.list[cIdx]) {
        Swal.fire("Hata", "Silinecek kısıt bulunamadı", "error");
        return;
      }
      let detail = data.list[cIdx].details;
      Swal.fire({
        title: "Kısıtı silmek istediğinize emin misiniz?",
        html: `<div class='mb-2'>${detail}</div>`,
        icon: "warning",
        showCancelButton: true,
        confirmButtonText: "Evet, Sil",
        cancelButtonText: "İptal",
      }).then((res) => {
        if (res.isConfirmed) {
          fetch("/Home/DeleteConstraint", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ Type: constraintType, Index: cIdx }),
          })
            .then((r) => r.json())
            .then((data) => {
              if (data.success) {
                Swal.fire("Başarılı", "Kısıt silindi", "success");
                setTimeout(() => openConstraintDialog(constraintType), 500);
              } else {
                Swal.fire("Hata", data.message || "Silme başarısız", "error");
              }
            });
        }
      });
    });
};

function generateTimetable() {
    Swal.fire({
        title: "Çizelge Oluşturuluyor",
        text: "Lütfen bekleyin...",
        allowOutsideClick: false,
        didOpen: () => {
            Swal.showLoading();
        }
    });

    fetch("/Home/Generate", {
        method: "POST"
    })
        .then(response => response.json())
        .then(data => {
            Swal.close();

            if (data.success) {
                Swal.fire({
                    icon: "success",
                    title: "Başarılı!",
                    html: `
                      ${data.message}<br/>
                      <a href="${data.downloadUrl}" class="btn btn-success mt-2" download>
                        Excel Dosyasını İndir
                      </a>
                    `
                });
            }
            else {
                Swal.fire({
                    icon: "error",
                    title: "Hata!",
                    text: data.message
                });
            }
        })
        .catch(error => {
            Swal.fire({
                icon: "error",
                title: "İşlem Başarısız",
                text: "Bir hata oluştu: " + error
            });
        });
}
