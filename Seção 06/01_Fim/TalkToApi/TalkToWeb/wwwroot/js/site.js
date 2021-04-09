function TesteCors() {

    var tokenJWT = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJlbWFpbCI6Imxlb3BhbnplckBnbWFpbC5jb20iLCJzdWIiOiJkZTMzNWZkZC01ZGZmLTRmOWMtOWQ5Yy1hZDBlYmZmN2Q4M2IiLCJleHAiOjE2MTc5NDA2MDR9.lDJAAbqK6vQdgaVbFNh2UI10DfxQoGcadyaLVO-_Q1M";
    var servico = "https://localhost:44316/api/mensagem/de335fdd-5dff-4f9c-9d9c-ad0ebff7d83b/06718c4e-d3e6-4b60-8a18-5e3c21ef5b74";

    $("#resultado").html("---Solicitando---");

    $.ajax({
        url: servico,
        method: "GET",
        crossDomain: true,
        headers: { "Accept": "application/json" },
        beforeSend: function (xhr) {
            xhr.setRequestHeader("Authorization", "Bearer " + tokenJWT);
        },
        success: function (data, status, xhr) {
            $("#resultado").html(data);
        }
    });

}