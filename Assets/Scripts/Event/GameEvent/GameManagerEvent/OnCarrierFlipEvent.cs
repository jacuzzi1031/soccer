
    public struct OnCarrierFlipEvent:IEvent {
        public bool carrierHeadingRight;

        public OnCarrierFlipEvent(bool carrierHeading) {
            this.carrierHeadingRight = carrierHeading;
        }
    }
