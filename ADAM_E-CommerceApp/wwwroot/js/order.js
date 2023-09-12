var dataTable;
$(document).ready(function () {
    var url = window.location.search;     
    switch (true) {       
        case url.includes("pending"):
            loadDataTable("pending");              
            break;
        case url.includes("approved"):
            loadDataTable("approved");           
            break;
        case url.includes("inprocess"):
            loadDataTable("inprocess");
            break;
        case url.includes("completed"):
            loadDataTable("completed");
            break;
        default:
            loadDataTable("all");
            break;
    }
});

function loadDataTable(status) {
    dataTable = $('#tblData').DataTable({
        'ajax': { url: '/admin/order/getall?status=' + status},    
        'columns':[
            { data: 'orderHeaderId', 'width': '5%', className: "text-center" },
            { data: 'applicationUser.name', 'width': '20%' },
            { data: 'applicationUser.email', 'width': '15%' },
            { data: 'applicationUser.phoneNumber', 'width': '10%', className: "text-center" },            
            { data: 'orderStatus', 'width': '15%', className: "text-center" },           
            { data: 'orderTotal', render: $.fn.dataTable.render.number(',', '.', 2,'$'), 'width': '10%', className: "text-center" },            
            {
                data: 'orderHeaderId',
                'render': function (data) {
                    return`<div class="py-0 text-center">
                        <a href="/admin/order/details?orderId=${data}" class="btn btn-outline-primary btn-sm mx-2 mb-0 py-0"><i class="bi bi-pencil-square"></i></a>                       
                    </div>`
                },
                'width': '15%'
            } 
        ]
    });    
}
