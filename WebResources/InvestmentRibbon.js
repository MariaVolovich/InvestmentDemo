var InvestmentRibbon = (function () {

    function confirmInvestment(primaryControl) {

        var formContext = primaryControl;

        var confirmStrings = {
            title: "Confirm Investment",
            text: "Are you sure you want to confirm this investment?"
        };

        var confirmOptions = {
            height: 200,
            width: 450
        };

        Xrm.Navigation.openConfirmDialog(confirmStrings, confirmOptions)
            .then(function (result) {

                if (!result.confirmed)
                    return;

                var CONFIRMED_STATUS = 100000001;

                formContext.getAttribute("mv_investmentstatus")
                    .setValue(CONFIRMED_STATUS);

                formContext.data.save();

            });
    }

    return {
        confirmInvestment: confirmInvestment
    };

})();