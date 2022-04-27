$(document).ready(function () {
    $(document).on("click", ".productdetail", function (e) {
        e.preventDefault();

        let url = $(this).attr("href");

        fetch(url).then(response => response.text())
            .then(data => {
                $(".modal-content").html(data)

                $('.quick-view-image').slick({
                    slidesToShow: 1,
                    slidesToScroll: 1,
                    arrows: false,
                    dots: false,
                    fade: true,
                    asNavFor: '.quick-view-thumb',
                    speed: 400,
                });

                $('.quick-view-thumb').slick({
                    slidesToShow: 4,
                    slidesToScroll: 1,
                    asNavFor: '.quick-view-image',
                    dots: false,
                    arrows: false,
                    focusOnSelect: true,
                    speed: 400,
                });

                $("#productQuickModal").modal("show")
            })
    })

    $(document).on("click", "#addbasketbtn", function (e) {
        e.preventDefault()

        let url = $("#basketform").attr("action")
        let count = $("#productcount").val();
        url = url + "?count=" + count;
        fetch(url).then(response =>
        {
            return response.text();
        }).then(data =>
        {
            $(".header-cart").html(data)

        })
    })

    $(document).on("click", ".addbasketlink", function (e) {
        e.preventDefault()

        let url = $(this).attr("href")
        fetch(url).then(response => {
            return response.text();
        }).then(data => {
            $(".header-cart").html(data)

        })
    })

    $(document).on("click", ".basketUpdate", function (e) {
        e.preventDefault();
        let url = $(this).attr("href");
        let count = $(this).parent().children()[1].value;
        count = parseInt(count);

        if ($(this).hasClass("subCount")) {
            count--;

        }
        else if ($(this).hasClass("addCount")) {
            count++;
        }
        $(this).parent().children()[1].value = count
        url = url + "?count=" + count;

        fetch(url).then(response => response.text()).then(data => $(".basketContainer").html(data))
    })

    $(document).on("keyup", ".basketItemCount", function () {
        let url = $(this).next().attr("href");
        url = url + "?count=" + $(this).val();

        if ($(this).val().trim()) {
            fetch(url).then(response => response.text()).then(data => $(".basketContainer").html(data))

        }
    })
    $(document).on("click", ".deleteproduct", function (e) {
        e.preventDefault();
        let url = $(this).attr("href");
        fetch(url).then(res => {
            return res.text();
        })
            .then(data => {
                $(".basketContainer").html(data)
            })
    })
})